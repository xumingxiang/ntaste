namespace org.apache.commons.math3.random
{
    public interface RandomGenerator
    {
        bool nextBoolean();

        void nextBytes(byte[] bytes);

        double nextDouble();

        float nextFloat();

        double nextGaussian();

        int nextInt();

        int nextInt(int n);

        long nextlong();

        void setSeed(int seed);

        void setSeed(int[] seed);

        void setSeed(long seed);
    }
}