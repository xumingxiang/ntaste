namespace org.apache.commons.math3.random
{
    using org.apache.commons.math3.exception;
    using org.apache.commons.math3.util;
    using System;

    public abstract class BitsStreamGenerator : RandomGenerator
    {
        private double _nextGaussian = double.NaN;
        private static readonly double minNonZeroDouble = Math.Pow(2.0, -52.0);
        private static long serialVersionUID = 0x1332938L;

        public void clear()
        {
            this._nextGaussian = double.NaN;
        }

        protected abstract int next(int bits);

        public bool nextBoolean()
        {
            return (this.next(1) != 0);
        }

        public void nextBytes(byte[] bytes)
        {
            int num3;
            int index = 0;
            int num2 = bytes.Length - 3;
            while (index < num2)
            {
                num3 = this.next(0x20);
                bytes[index] = (byte)(num3 & 0xff);
                bytes[index + 1] = (byte)((num3 >> 8) & 0xff);
                bytes[index + 2] = (byte)((num3 >> 0x10) & 0xff);
                bytes[index + 3] = (byte)((num3 >> 0x18) & 0xff);
                index += 4;
            }
            for (num3 = this.next(0x20); index < bytes.Length; num3 = num3 >> 8)
            {
                bytes[index++] = (byte)(num3 & 0xff);
            }
        }

        public double nextDouble()
        {
            long num = this.next(0x1a) << 0x1a;
            long num2 = this.next(0x1a);
            return ((num | num2) * minNonZeroDouble);
        }

        public float nextFloat()
        {
            return (this.next(0x17) * 1E-23f);
        }

        public double nextGaussian()
        {
            double num;
            if (double.IsNaN(this._nextGaussian))
            {
                double num2 = this.nextDouble();
                double x = this.nextDouble();
                double d = 6.2831853071795862 * num2;
                double num5 = Math.Sqrt(-2.0 * MathUtil.Log(x));
                num = num5 * Math.Cos(d);
                this._nextGaussian = num5 * Math.Sin(d);
                return num;
            }
            num = this._nextGaussian;
            this._nextGaussian = double.NaN;
            return num;
        }

        public int nextInt()
        {
            return this.next(0x20);
        }

        public int nextInt(int n)
        {
            int num;
            int num2;
            if (n <= 0)
            {
                throw new NotStrictlyPositiveException(n);
            }
            if ((n & -n) == n)
            {
                return ((n * this.next(0x1f)) >> 0x1f);
            }
            do
            {
                num = this.next(0x1f);
                num2 = num % n;
            }
            while (((num - num2) + (n - 1)) < 0);
            return num2;
        }

        public long nextlong()
        {
            long num = this.next(0x20) << 0x20;
            long num2 = this.next(0x20) & ((long)0xffffffffL);
            return (num | num2);
        }

        public long nextlong(long n)
        {
            long num;
            long num2;
            if (n <= 0L)
            {
                throw new NotStrictlyPositiveException(n);
            }
            do
            {
                num = this.next(0x1f) << 0x20;
                num |= this.next(0x20) & ((long)0xffffffffL);
                num2 = num % n;
            }
            while (((num - num2) + (n - 1L)) < 0L);
            return num2;
        }

        public abstract void setSeed(int[] seed);

        public abstract void setSeed(int seed);

        public abstract void setSeed(long seed);
    }
}