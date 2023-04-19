﻿namespace Runner.ByteImpl
{
    public sealed class GaloisField256
    {
        private readonly int size;

        private readonly byte[] field;

        private readonly byte[] logarithms;

        private readonly byte[] inverses;

        private readonly byte[,] multTable;

        public GaloisField256(int size, int fieldGenPoly)
        {
            if (size > 256)
            {
                throw new ArgumentOutOfRangeException(
                    "size", "must be less than or equal to '256', the largest field this " +
                            "particular implementation is able to handle."
                );
            }

            if (size < 2)
            {
                throw new ArgumentOutOfRangeException("size", "must be >= 2");
            }

            this.size = size;

            this.field = new byte[this.size];
            this.logarithms = new byte[this.size];
            this.multTable = new byte[this.size, this.size];
            this.inverses = new byte[this.size];

            BuildField(fieldGenPoly);
            BuildLogarithms();
            BuildMultTable();
            BuildInverses();
        }

        public byte[] Field
        {
            get { return this.field; }
        }

        public byte[] Inverses
        {
            get { return this.inverses; }
        }

        public byte[] Logarithms
        {
            get { return this.logarithms; }
        }

        public byte Multiply(byte left, byte right)
        {
            return this.multTable[left, right];
        }

        public byte Divide(byte dividend, byte divisor)
        {
            return this.multTable[dividend, this.inverses[divisor]];
        }

        public byte[] PolyMult(byte[] left, byte[] right)
        {
            byte[] result;
            byte coeff;

            result = new byte[left.Length + right.Length - 1];

            for (int leftIndex = 0; leftIndex < left.Length; leftIndex++)
            {
                for (int rightIndex = 0; rightIndex < right.Length; rightIndex++)
                {
                    coeff = Multiply(left[leftIndex], right[rightIndex]);

                    result[leftIndex + rightIndex] = (byte)(result[leftIndex + rightIndex] ^ coeff);
                }
            }

            return result;
        }

        public byte PolyEval(byte[] poly, byte x)
        {
            int sum;
            int xLog;
            int coeffLog;
            int power;

            sum = poly[0];

            xLog = this.logarithms[x];

            for (int i = 1; i < poly.Length; i++)
            {
                if (poly[i] == 0)
                {
                    continue;
                }

                coeffLog = this.logarithms[poly[i]];

                power = (coeffLog + xLog * i) % (size - 1);
                //power = (byte)FastMod( coeffLog + xLog * i, size - 1 );

                sum ^= this.field[power + 1];
            }

            return (byte)sum;
        }

        private static int FastMod(int operand, int modulus)
        {
            while (operand >= modulus)
            {
                operand -= modulus;
            }

            return operand;
        }

        private void BuildField(int fieldGenPoly)
        {
            int curr;
            int last;

            this.field[0] = 0;
            this.field[1] = 1;

            last = 1;

            for (int i = 2; i < this.size; i++)
            {
                curr = last << 1;

                if (curr >= size)
                {
                    curr = curr ^ fieldGenPoly;
                }

                this.field[i] = (byte)curr;

                last = curr;
            }
        }

        private void BuildLogarithms()
        {
            for (int i = 0; i < this.Field.Length; i++)
            {
                this.logarithms[this.field[i]] = (byte)(i - 1);
            }
        }

        private void BuildMultTable()
        {
            // These loop indexes, and InternalMult, all take `int`s. This is required.
            // If I used byte for this, then when size == 256 (valid for an 8-bit field), the iteration
            // variables overflow from 255 --> 0, and so the loop runs for infinity. Using ints still gives
            // the right values.
            for (int left = 0; left < this.size; left++)
            {
                for (int right = 0; right < this.size; right++)
                {
                    this.multTable[left, right] = InternalMult(left, right);
                }
            }
        }

        private void BuildInverses()
        {
            this.inverses[0] = 0;
            for (int i = 1; i < this.inverses.Length; i++)
            {
                this.inverses[this.Field[i]] = InternalDivide(1, this.Field[i]);
            }
        }

        private byte InternalMult(int left, int right)
        {
            if (left == 0 || right == 0)
            {
                return 0;
            }

            int value = this.logarithms[left] + this.logarithms[right];

            value = value % (size - 1);

            return this.field[value + 1];
        }

        private byte InternalDivide(byte dividend, byte divisor)
        {
            if (dividend == 0)
            {
                return 0;
            }

            int value = this.Logarithms[dividend] - this.Logarithms[divisor] + (size - 1);

            value = value % (size - 1);

            return this.Field[value + 1];
        }
    }
}