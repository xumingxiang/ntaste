namespace org.apache.commons.math3.random
{
    using System;
    using System.Runtime.CompilerServices;

    public class MersenneTwister : BitsStreamGenerator
    {
        private static int M = 0x18d;
        private static int[] MAG01;
        private int[] mt;
        private int mti;
        private static int N = 0x270;

        static MersenneTwister()
        {
            int[] numArray = new int[2];
            numArray[1] = -1727483681;
            MAG01 = numArray;
        }

        public MersenneTwister()
        {
            this.mt = new int[N];
            this.setSeed((int)(Environment.TickCount + RuntimeHelpers.GetHashCode(this)));
        }

        public MersenneTwister(int[] seed)
        {
            this.mt = new int[N];
            this.setSeed(seed);
        }

        public MersenneTwister(int seed)
        {
            this.mt = new int[N];
            this.setSeed(seed);
        }

        public MersenneTwister(long seed)
        {
            this.mt = new int[N];
            this.setSeed(seed);
        }

        protected override int next(int bits)
        {
            int num;
            if (this.mti >= N)
            {
                int num3;
                int num4;
                int num2 = this.mt[0];
                for (num3 = 0; num3 < (N - M); num3++)
                {
                    num4 = num2;
                    num2 = this.mt[num3 + 1];
                    num = (num4 & -2147483648) | (num2 & 0x7fffffff);
                    this.mt[num3] = (this.mt[num3 + M] ^ (num >> 1)) ^ MAG01[num & 1];
                }
                for (num3 = N - M; num3 < (N - 1); num3++)
                {
                    num4 = num2;
                    num2 = this.mt[num3 + 1];
                    num = (num4 & -2147483648) | (num2 & 0x7fffffff);
                    this.mt[num3] = (this.mt[num3 + (M - N)] ^ (num >> 1)) ^ MAG01[num & 1];
                }
                num = (num2 & -2147483648) | (this.mt[0] & 0x7fffffff);
                this.mt[N - 1] = (this.mt[M - 1] ^ (num >> 1)) ^ MAG01[num & 1];
                this.mti = 0;
            }
            num = this.mt[this.mti++];
            num ^= num >> 11;
            num ^= (num << 7) & -1658038656;
            num ^= (num << 15) & -272236544;
            num ^= num >> 0x12;
            return (num >> (0x20 - bits));
        }

        public override void setSeed(long seed)
        {
            this.setSeed(new int[] { (int)seed, (int)(((ulong)seed) & 0xffffffffL) });
        }

        public override void setSeed(int[] seed)
        {
            if (seed == null)
            {
                this.setSeed((int)(Environment.TickCount + RuntimeHelpers.GetHashCode(this)));
            }
            else
            {
                int num3;
                long num4;
                long num5;
                long num6;
                this.setSeed(0x12bd6aa);
                int index = 1;
                int num2 = 0;
                for (num3 = Math.Max(N, seed.Length); num3 != 0; num3--)
                {
                    num4 = (this.mt[index] & 0x7fffffffL) | ((this.mt[index] < 0) ? ((long)0x80000000L) : 0L);
                    num5 = (this.mt[index - 1] & 0x7fffffffL) | ((this.mt[index - 1] < 0) ? ((long)0x80000000L) : 0L);
                    num6 = ((num4 ^ ((num5 ^ (num5 >> 30)) * 0x19660dL)) + seed[num2]) + num2;
                    this.mt[index] = (int)(((ulong)num6) & 0xffffffffL);
                    index++;
                    num2++;
                    if (index >= N)
                    {
                        this.mt[0] = this.mt[N - 1];
                        index = 1;
                    }
                    if (num2 >= seed.Length)
                    {
                        num2 = 0;
                    }
                }
                for (num3 = N - 1; num3 != 0; num3--)
                {
                    num4 = (this.mt[index] & 0x7fffffffL) | ((this.mt[index] < 0) ? ((long)0x80000000L) : 0L);
                    num5 = (this.mt[index - 1] & 0x7fffffffL) | ((this.mt[index - 1] < 0) ? ((long)0x80000000L) : 0L);
                    num6 = (num4 ^ ((num5 ^ (num5 >> 30)) * 0x5d588b65L)) - index;
                    this.mt[index] = (int)(((ulong)num6) & 0xffffffffL);
                    index++;
                    if (index >= N)
                    {
                        this.mt[0] = this.mt[N - 1];
                        index = 1;
                    }
                }
                this.mt[0] = -2147483648;
                base.clear();
            }
        }

        public override void setSeed(int seed)
        {
            long num = seed;
            this.mt[0] = (int)num;
            this.mti = 1;
            while (this.mti < N)
            {
                num = ((0x6c078965L * (num ^ (num >> 30))) + this.mti) & ((long)0xffffffffL);
                this.mt[this.mti] = (int)num;
                this.mti++;
            }
            base.clear();
        }
    }
}