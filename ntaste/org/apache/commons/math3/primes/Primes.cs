namespace org.apache.commons.math3.primes
{
    using System;
    using System.Collections.Generic;

    public class Primes
    {
        private Primes()
        {
        }

        public static bool isPrime(int n)
        {
            if (n < 2)
            {
                return false;
            }
            foreach (int num in SmallPrimes.PRIMES)
            {
                if (0 == (n % num))
                {
                    return (n == num);
                }
            }
            return SmallPrimes.millerRabinPrimeTest(n);
        }

        public static int nextPrime(int n)
        {
            if (n < 0)
            {
                throw new ArgumentException();
            }
            if (n == 2)
            {
                return 2;
            }
            n |= 1;
            if (n == 1)
            {
                return 2;
            }
            if (isPrime(n))
            {
                return n;
            }
            int num = n % 3;
            if (0 == num)
            {
                n += 2;
            }
            else if (1 == num)
            {
                n += 4;
            }
            while (true)
            {
                if (isPrime(n))
                {
                    return n;
                }
                n += 2;
                if (isPrime(n))
                {
                    return n;
                }
                n += 4;
            }
        }

        public static List<int> primeFactors(int n)
        {
            if (n < 2)
            {
                throw new ArgumentException();
            }
            return SmallPrimes.trialDivision(n);
        }
    }
}