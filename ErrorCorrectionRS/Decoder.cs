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
        BlockSize = fieldSize - 1;

        this.fieldGenPoly = fieldGenPoly;

        gf = new GaloisField(fieldSize, fieldGenPoly);

        syndroms = new int[paritySymbols];

        lambda = new int[paritySymbols - 1];
        corrPoly = new int[paritySymbols - 1];
        lambdaStar = new int[paritySymbols - 1];

        lambdaPrime = new int[paritySymbols - 2];

        omega = new int[paritySymbols - 2];

        errorIndexes = new int[fieldSize - 1];

        chienCache = new int[fieldSize - 1];

        for (int i = 0; i < chienCache.Length; i++)
        {
            chienCache[i] = gf.Inverses[gf.Field[i + 1]];
        }
    }

    public int BlockSize { get; private set; }

    public int MessageSize => messageSymbols;

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