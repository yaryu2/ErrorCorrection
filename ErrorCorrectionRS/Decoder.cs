using ErrorCorrection;

namespace ErrorCorrectionRS;

public sealed class Decoder
{
    private readonly int fieldSize;
    private readonly int messageSymbols;
    private readonly int paritySymbols;

    private readonly int fieldGenPoly;

    private readonly GaloisField gf;

    private readonly int[] syndroms;

    private readonly int[] lambda;
    private readonly int[] corrPoly;
    private readonly int[] lambdaStar;

    private readonly int[] lambdaPrime;

    private readonly int[] omega;

    private readonly int[] errorIndexes;

    private readonly int[] chienCache;

    /// <summary>
    /// Initializes a new instance of the reed-solomon decoder.
    /// </summary>
    /// <param name="fieldSize">The size of the Galois field to create. Must be a value that is 
    /// a power of two. The length of the output block is set to `fieldSize - 1`.</param>
    /// <param name="messageSymbols">The number of original message symbols per block.</param>
    /// <param name="paritySymbols">The number of parity symbols per block.</param>
    /// <param name="fieldGenPoly">A value representing the field generator polynomial, 
    /// which must be order N for a field GF(2^N).</param>
    /// <remarks>
    /// BlockSize is equal to `fieldSize - 1`. messageSymbols plus paritySymbols must equal BlockSize.
    /// </remarks>
    public Decoder(int fieldSize, int messageSymbols, int paritySymbols, int fieldGenPoly)
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
        this.BlockSize = fieldSize - 1;

        this.fieldGenPoly = fieldGenPoly;

        this.gf = new GaloisField(fieldSize, fieldGenPoly);

        // Syndrom calculation buffers
        this.syndroms = new int[paritySymbols];

        // Lamda calculation buffers
        this.lambda = new int[paritySymbols - 1];
        this.corrPoly = new int[paritySymbols - 1];
        this.lambdaStar = new int[paritySymbols - 1];

        // LambdaPrime calculation buffers
        this.lambdaPrime = new int[paritySymbols - 2];

        // Omega calculation buffers
        this.omega = new int[paritySymbols - 2];

        // Error position calculation
        this.errorIndexes = new int[fieldSize - 1];

        // Cache of the lookup used in the ChienSearch process.
        this.chienCache = new int[fieldSize - 1];

        for (int i = 0; i < this.chienCache.Length; i++)
        {
            this.chienCache[i] = gf.Inverses[gf.Field[i + 1]];
        }
    }

    /// <summary>
    /// The number of symbols that make up an entire encoded message. An encoded message is composed of the
    /// original data symbols plus parity symbols.
    /// </summary>
    public int BlockSize { get; private set; }

    /// <summary>
    /// The number of symbols per block that store original message symbols.
    /// </summary>
    public int MessageSize
    {
        get { return this.messageSymbols; }
    }

    /// <summary>
    /// Discovers and corrects any errors in the block provided.
    /// </summary>
    /// <param name="message">A block containing a reed-solomon encoded message.</param>
    public void Decode(int[] message)
    {
        if (message.Length != this.BlockSize)
        {
            throw new ArgumentException("The provided message's size was not the size of a block");
        }

        CalcSyndromPoly(message);
        CalcLambda();
        CalcLambdaPrime();
        CalcOmega();

        ChienSearch();

        RepairErrors(message, errorIndexes, omega, lambdaPrime);
    }

    private void RepairErrors(int[] message, int[] errorIndexes, int[] omega, int[] lp)
    {
        int top, bottom, x, xInverse, messageLen = message.Length;

        for (int i = 0; i < messageLen; i++)
        {
            if (errorIndexes[i] == 0)
            {
                x = gf.Field[i + 1];

                xInverse = gf.Inverses[x];

                top = gf.PolyEval(omega, xInverse);
                top = gf.Multiply(top, x);
                bottom = gf.PolyEval(lp, xInverse);

                message[i] ^= gf.Divide(top, bottom);
            }
        }
    }

    private void CalcLambda()
    {
        int k, l, e, eInv;

        Array.Clear(corrPoly, 0, corrPoly.Length);
        Array.Clear(lambda, 0, lambda.Length);
        k = 1;
        l = 0;
        corrPoly[1] = 1;
        lambda[0] = 1;


        while (k <= paritySymbols)
        {
            e = syndroms[k - 1];

            for (int i = 1; i <= l; i++)
            {
                e ^= gf.Multiply(lambda[i], syndroms[k - 1 - i]);
            }

            if (e != 0)
            {
                for (int i = 0; i < lambdaStar.Length; i++)
                {
                    lambdaStar[i] = lambda[i] ^ gf.Multiply(e, corrPoly[i]);
                }

                if (2 * l < k)
                {
                    l = k - l;

                    eInv = gf.Inverses[e];
                    for (int i = 0; i < corrPoly.Length; i++)
                    {
                        corrPoly[i] = gf.Multiply(lambda[i], eInv);
                    }
                }
            }

            for (int i = corrPoly.Length - 1; i >= 1; i--)
            {
                corrPoly[i] = corrPoly[i - 1];
            }

            corrPoly[0] = 0;

            if (e != 0)
            {
                Array.Copy(lambdaStar, lambda, lambda.Length);
            }

            k += 1;
        }
    }

    private void CalcLambdaPrime()
    {
        for (int i = 0; i < lambdaPrime.Length; i++)
        {
            if ((i & 0x1) == 0)
            {
                lambdaPrime[i] = lambda[i + 1];
            }
            else
            {
                lambdaPrime[i] = 0;
            }
        }
    }

    private void CalcOmega()
    {
        for (int i = 0; i < omega.Length; i++)
        {
            omega[i] = syndroms[i];

            for (int lIter = 1; lIter <= i; lIter++)
            {
                omega[i] ^= gf.Multiply(syndroms[i - lIter], lambda[lIter]);
            }
        }
    }

    private void ChienSearch()
    {
        for (int i = 0; i < errorIndexes.Length; i++)
        {
            errorIndexes[i] = gf.PolyEval(lambda, chienCache[i]);
        }
    }


    private void CalcSyndromPoly(int[] message)
    {
        int syndrome, root;

        for (int synIndex = 0; synIndex < syndroms.Length; synIndex++)
        {
            root = gf.Field[synIndex + 1];
            syndrome = 0;

            for (int i = message.Length - 1; i > 0; i--)
            {
                syndrome = gf.Multiply((syndrome ^ message[i]), root);
            }

            syndroms[synIndex] = syndrome ^ message[0];
        }
    }
}