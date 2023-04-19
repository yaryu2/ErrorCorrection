using System.Text;

namespace ErrorCorrectionRS;

public sealed class GaloisField
{
    public readonly int[] Field;
    public readonly int[] Inverses;
    public readonly int[] Logarithms;
    private int fieldGenPoly;
    private int[,] multTable;
    private int size;


    public GaloisField(int size, int fieldGenPoly)
    {
        this.size = size;
        this.fieldGenPoly = fieldGenPoly;

        Field = new int[size];
        Logarithms = new int[Field.Length];
        Inverses = new int[Field.Length];

        BuildField();
        BuildLogarithms();
        BuildMultTable();
        BuildInverses();
    }

    public int Divide(int dividend, int divisor)
    {
        return multTable[dividend, Inverses[divisor]];
    }

    public int Multiply(int left, int right)
    {
        return multTable[left, right];
    }

    public int PolyEval(int[] poly, int x)
    {
        int coeffLog, power, sum = poly[0], xLog = Logarithms[x];

        for (int i = 1; i < poly.Length; i++)
        {
            if (poly[i] == 0)
            {
                continue;
            }

            coeffLog = Logarithms[poly[i]];

            power = (coeffLog + xLog * i) % (size - 1);
            sum ^= Field[power + 1];
        }

        return sum;
    }
    
    public int[] PolyMult(int[] left, int[] right)
    {
        int[] result;
        int coeff;
        result = new int[left.Length + right.Length - 1];

        for (int i = 0; i < left.Length; i++)
        {
            for (int j = 0; j < right.Length; j++)
            {
                coeff = InternalMult(left[i], right[j]);

                result[i + j] ^= coeff;
            }
        }

        return result;
    }

    private void BuildField()
    {
        int next, last = 1;

        Field[0] = 0;
        Field[1] = 1;

        for (int i = 2; i < Field.Length; i++)
        {
            next = last << 1;

            if (next >= size)
            {
                next = next ^ fieldGenPoly;
            }

            Field[i] = next;
            last = next;
        }
    }

    private void BuildInverses()
    {
        Inverses[0] = 0;
        for (int i = 1; i < Inverses.Length; i++)
        {
            Inverses[Field[i]] = InternalDivide(1, Field[i]);
        }
    }

    private void BuildLogarithms()
    {
        for (int i = 0; i < Field.Length; i++)
        {
            Logarithms[Field[i]] = i - 1;
        }
    }

    private void BuildMultTable()
    {
        multTable = new int[size, size];

        for (int left = 0; left < size; left++)
        {
            for (int right = 0; right < size; right++)
            {
                this.multTable[left, right] = InternalMult(left, right);
            }
        }
    }

    private int InternalDivide(int dividend, int divisor)
    {
        if (dividend == 0)
        {
            return 0;
        }

        dividend = Logarithms[dividend];

        divisor = Logarithms[divisor];

        dividend = (dividend - divisor + (size - 1)) % (size - 1);

        return Field[dividend + 1];
    }

    private int InternalMult(int left, int right)
    {
        if (left == 0 || right == 0)
        {
            return 0;
        }

        left = Logarithms[left];
        right = Logarithms[right];

        left = (left + right) % (size - 1);

        return Field[left + 1];
    }
}