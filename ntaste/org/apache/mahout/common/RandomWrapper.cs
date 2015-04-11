namespace org.apache.mahout.common
{
    using org.apache.commons.math3.random;
    using System;
    using System.Runtime.CompilerServices;

    public sealed class RandomWrapper
    {
        private RandomGenerator random;
        private static long STANDARD_SEED = -3819370596149511490L;

        public RandomWrapper()
        {
            this.random = new MersenneTwister();
            this.random.setSeed((int)(Environment.TickCount + RuntimeHelpers.GetHashCode(this)));
        }

        public RandomWrapper(long seed)
        {
            this.random = new MersenneTwister(seed);
        }

        public RandomGenerator getRandomGenerator()
        {
            return this.random;
        }

        protected int next(int bits)
        {
            throw new NotSupportedException();
        }

        public bool nextBoolean()
        {
            return this.random.nextBoolean();
        }

        public void nextBytes(byte[] bytes)
        {
            this.random.nextBytes(bytes);
        }

        public double nextDouble()
        {
            return this.random.nextDouble();
        }

        public float nextFloat()
        {
            return this.random.nextFloat();
        }

        public double nextGaussian()
        {
            return this.random.nextGaussian();
        }

        public int nextInt()
        {
            return this.random.nextInt();
        }

        public int nextInt(int n)
        {
            return this.random.nextInt(n);
        }

        public long nextlong()
        {
            return this.random.nextlong();
        }

        public void resetToTestSeed()
        {
            this.setSeed(STANDARD_SEED);
        }

        public void setSeed(long seed)
        {
            if (this.random != null)
            {
                this.random.setSeed(seed);
            }
        }
    }
}