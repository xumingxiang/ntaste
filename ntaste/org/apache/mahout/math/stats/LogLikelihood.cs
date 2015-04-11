namespace org.apache.mahout.math.stats
{
    using org.apache.commons.math3.util;
    using System;

    public sealed class LogLikelihood
    {
        private LogLikelihood()
        {
        }

        public static double entropy(params long[] elements)
        {
            long x = 0L;
            double num2 = 0.0;
            foreach (long num3 in elements)
            {
                num2 += xLogX(num3);
                x += num3;
            }
            return (xLogX(x) - num2);
        }

        private static double entropy(long a, long b)
        {
            return ((xLogX(a + b) - xLogX(a)) - xLogX(b));
        }

        private static double entropy(long a, long b, long c, long d)
        {
            return ((((xLogX(((a + b) + c) + d) - xLogX(a)) - xLogX(b)) - xLogX(c)) - xLogX(d));
        }

        public static double logLikelihoodRatio(long k11, long k12, long k21, long k22)
        {
            double num = entropy(k11 + k12, k21 + k22);
            double num2 = entropy(k11 + k21, k12 + k22);
            double num3 = entropy(k11, k12, k21, k22);
            if ((num + num2) < num3)
            {
                return 0.0;
            }
            return (2.0 * ((num + num2) - num3));
        }

        public static double rootLogLikelihoodRatio(long k11, long k12, long k21, long k22)
        {
            double num2 = Math.Sqrt(logLikelihoodRatio(k11, k12, k21, k22));
            if ((((double)k11) / ((double)(k11 + k12))) < (((double)k21) / ((double)(k21 + k22))))
            {
                num2 = -num2;
            }
            return num2;
        }

        private static double xLogX(long x)
        {
            return ((x == 0L) ? 0.0 : (x * MathUtil.Log((double)x)));
        }
    }
}