namespace org.apache.mahout.common
{
    using org.apache.commons.math3.primes;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public sealed class RandomUtils
    {
        private static IDictionary<RandomWrapper, bool> INSTANCES = new ConcurrentDictionary<RandomWrapper, bool>();
        public const int MAX_INT_SMALLER_TWIN_PRIME = 0x7ffffd45;
        private static bool testSeed = false;

        private RandomUtils()
        {
        }

        public static RandomWrapper getRandom()
        {
            RandomWrapper wrapper = new RandomWrapper();
            if (testSeed)
            {
                wrapper.resetToTestSeed();
            }
            INSTANCES[wrapper] = true;
            return wrapper;
        }

        public static RandomWrapper getRandom(long seed)
        {
            RandomWrapper wrapper = new RandomWrapper(seed);
            INSTANCES[wrapper] = true;
            return wrapper;
        }

        public static int hashDouble(double value)
        {
            return BitConverter.DoubleToInt64Bits(value).GetHashCode();
        }

        public static int hashFloat(float value)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
        }

        public static int nextTwinPrime(int n)
        {
            if (n > 0x7ffffd45)
            {
                throw new ArgumentException();
            }
            if (n <= 3)
            {
                return 5;
            }
            int num = Primes.nextPrime(n);
            while (!Primes.isPrime(num + 2))
            {
                num = Primes.nextPrime(num + 4);
            }
            return (num + 2);
        }

        public static void useTestSeed()
        {
            testSeed = true;
            lock (INSTANCES)
            {
                foreach (RandomWrapper wrapper in INSTANCES.Keys)
                {
                    wrapper.resetToTestSeed();
                }
            }
        }
    }
}