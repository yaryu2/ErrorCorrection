namespace ErrorCorrectionRS;

public sealed class Encoder
{
    private readonly int fieldSize;
    private readonly int messageSymbols;
    private readonly int paritySymbols;
    private readonly int[] codeGenPoly;
    private readonly int[] modulusResult;
    private readonly GaloisField gf;
    
    public Encoder(int fieldSize, int messageSymbols, int paritySymbols, int fieldGenPoly)
    {
        if (fieldSize - 1 != messageSymbols + paritySymbols)
        {
            throw new ArgumentOutOfRangeException(
                "Invalid reed-solomon block parameters were provided - " +
                "the number of message symbols plus the number of parity symbols " +
                "does not add up to the size of a block"
            );
        }

        this.fieldSize = fieldSize;
        this.messageSymbols = messageSymbols;
        this.paritySymbols = paritySymbols;
        BlockSize = fieldSize - 1;

        gf = new GaloisField(fieldSize, fieldGenPoly);

        codeGenPoly = BuildCodeGenPoly();

        modulusResult = new int[paritySymbols];
    }
    
    public int BlockSize { get; private set; }
    public int MessageSize => messageSymbols;

    private int[] BuildCodeGenPoly()
    {
        int numElements = fieldSize - messageSymbols - 1;

        List<int[]> polys = new List<int[]>((int)numElements);

        for (int i = 0; i < numElements; i++)
        {
            polys.Add(new int[] { gf.Field[i + 1], 1 });
        }
        
        int[] current = polys[0];
        for (int i = 1; i < numElements; i++)
        {
            current = gf.PolyMult(current, polys[(int)i]);
        }

        return current;
    }
    
    public void Encode(int[] message)
    {
        int z_0;
        int r;
        int[] z = this.modulusResult;
        int[] g = this.codeGenPoly;

        Array.Clear(message, 0, paritySymbols);
        Array.Clear(z, 0, z.Length);
        z_0 = 0;
        
        for (int i = message.Length - 1; i > paritySymbols; i--)
        {
            r = z_0 ^ message[i];

            for (int zIter = 0; zIter < z.Length; zIter++)
            {
                z[zIter] ^= gf.Multiply(g[zIter], r);
            }

            z_0 = z[z.Length - 1];

            for (int zIter = z.Length - 1; zIter >= 1; zIter--)
            {
                z[zIter] = z[zIter - 1];
            }

            z[0] = 0;
        }
        
        r = z_0 ^ message[paritySymbols];
        
        for (int zIter = 0; zIter < z.Length; zIter++)
        {
            message[zIter] = z[zIter] ^ gf.Multiply(g[zIter], r);
        }
    }
}