namespace org.apache.commons.math3.distribution
{
    using org.apache.commons.math3.exception;
    using org.apache.commons.math3.random;
    using org.apache.commons.math3.special;
    using org.apache.commons.math3.util;
    using System;

    public class PascalDistribution : AbstractIntegerDistribution
    {
        private double log1mProbabilityOfSuccess;
        private double logProbabilityOfSuccess;
        private int numberOfSuccesses;
        private double probabilityOfSuccess;
        private static long serialVersionUID = 0x5db17834c1f59437L;

        public PascalDistribution(int r, double p)
            : this(new MersenneTwister(), r, p)
        {
        }

        public PascalDistribution(RandomGenerator rng, int r, double p)
            : base(rng)
        {
            if (r <= 0)
            {
                throw new NotStrictlyPositiveException(r);
            }
            if ((p < 0.0) || (p > 1.0))
            {
                throw new ArgumentOutOfRangeException("p", p, "(0, 1)");
            }
            this.numberOfSuccesses = r;
            this.probabilityOfSuccess = p;
            this.logProbabilityOfSuccess = MathUtil.Log(p);
            this.log1mProbabilityOfSuccess = MathUtil.Log1p(-p);
        }

        public long binomialCoefficient(int n, int k)
        {
            int num2;
            int num3;
            long num4;
            if ((n == k) || (k == 0))
            {
                return 1L;
            }
            if ((k == 1) || (k == (n - 1)))
            {
                return (long)n;
            }
            if (k > (n / 2))
            {
                return this.binomialCoefficient(n, n - k);
            }
            long num = 1L;
            if (n <= 0x3d)
            {
                num2 = (n - k) + 1;
                for (num3 = 1; num3 <= k; num3++)
                {
                    num = (num * num2) / ((long)num3);
                    num2++;
                }
                return num;
            }
            if (n <= 0x42)
            {
                num2 = (n - k) + 1;
                for (num3 = 1; num3 <= k; num3++)
                {
                    num4 = this.gcd(num2, num3);
                    num = (num / (((long)num3) / num4)) * (((long)num2) / num4);
                    num2++;
                }
                return num;
            }
            num2 = (n - k) + 1;
            for (num3 = 1; num3 <= k; num3++)
            {
                num4 = this.gcd(num2, num3);
                num = mulAndCheck((int)(num / (((long)num3) / num4)), (int)(((long)num2) / num4));
                num2++;
            }
            return num;
        }

        public double binomialCoefficientDouble(int n, int k)
        {
            if ((n == k) || (k == 0))
            {
                return 1.0;
            }
            if ((k == 1) || (k == (n - 1)))
            {
                return (double)n;
            }
            if (k > (n / 2))
            {
                return this.binomialCoefficientDouble(n, n - k);
            }
            if (n < 0x43)
            {
                return (double)this.binomialCoefficient(n, k);
            }
            double num = 1.0;
            for (int i = 1; i <= k; i++)
            {
                num *= ((double)((n - k) + i)) / ((double)i);
            }
            return Math.Floor((double)(num + 0.5));
        }

        public double binomialCoefficientLog(int n, int k)
        {
            int num2;
            if ((n == k) || (k == 0))
            {
                return 0.0;
            }
            if ((k == 1) || (k == (n - 1)))
            {
                return MathUtil.Log((double)n);
            }
            if (n < 0x43)
            {
                return MathUtil.Log((double)this.binomialCoefficient(n, k));
            }
            if (n < 0x406)
            {
                return MathUtil.Log(this.binomialCoefficientDouble(n, k));
            }
            if (k > (n / 2))
            {
                return this.binomialCoefficientLog(n, n - k);
            }
            double num = 0.0;
            for (num2 = (n - k) + 1; num2 <= n; num2++)
            {
                num += MathUtil.Log((double)num2);
            }
            for (num2 = 2; num2 <= k; num2++)
            {
                num -= MathUtil.Log((double)num2);
            }
            return num;
        }

        public override double cumulativeProbability(int x)
        {
            if (x < 0)
            {
                return 0.0;
            }
            return Beta.regularizedBeta(this.probabilityOfSuccess, (double)this.numberOfSuccesses, x + 1.0);
        }

        public int gcd(int p, int q)
        {
            int a = p;
            int b = q;
            if ((a == 0) || (b == 0))
            {
                if ((a == -2147483648) || (b == -2147483648))
                {
                    throw new ArithmeticException(string.Format("GCD_OVERFLOW_32_BITS {0},{1}", p, q));
                }
                return Math.Abs((int)(a + b));
            }
            long num3 = a;
            long num4 = b;
            bool flag = false;
            if (a < 0)
            {
                if (-2147483648 == a)
                {
                    flag = true;
                }
                else
                {
                    a = -a;
                }
                num3 = -num3;
            }
            if (b < 0)
            {
                if (-2147483648 == b)
                {
                    flag = true;
                }
                else
                {
                    b = -b;
                }
                num4 = -num4;
            }
            if (flag)
            {
                if (num3 == num4)
                {
                    throw new ArithmeticException(string.Format("GCD_OVERFLOW_32_BITS {0},{1}", p, q));
                }
                long num5 = num4;
                num4 = num3;
                num3 = num5 % num3;
                if (num3 == 0L)
                {
                    if (num4 > -2147483648L)
                    {
                        throw new ArithmeticException(string.Format("GCD_OVERFLOW_32_BITS {0},{1}", p, q));
                    }
                    return (int)num4;
                }
                num5 = num4;
                b = (int)num3;
                a = (int)(num5 % num3);
            }
            return this.gcdPositive(a, b);
        }

        private int gcdPositive(int a, int b)
        {
            if (a == 0)
            {
                return b;
            }
            if (b == 0)
            {
                return a;
            }
            int num = MathUtil.NumberOfTrailingZeros(a);
            a = a >> num;
            int num2 = MathUtil.NumberOfTrailingZeros(b);
            b = b >> num2;
            int num3 = Math.Min(num, num2);
            while (a != b)
            {
                int num4 = a - b;
                b = Math.Min(a, b);
                a = Math.Abs(num4);
                a = a >> MathUtil.NumberOfTrailingZeros(a);
            }
            return (a << num3);
        }

        public int getNumberOfSuccesses()
        {
            return this.numberOfSuccesses;
        }

        public override double getNumericalMean()
        {
            double num = this.getProbabilityOfSuccess();
            double num2 = this.getNumberOfSuccesses();
            return ((num2 * (1.0 - num)) / num);
        }

        public override double getNumericalVariance()
        {
            double num = this.getProbabilityOfSuccess();
            double num2 = this.getNumberOfSuccesses();
            return ((num2 * (1.0 - num)) / (num * num));
        }

        public double getProbabilityOfSuccess()
        {
            return this.probabilityOfSuccess;
        }

        public override int getSupportLowerBound()
        {
            return 0;
        }

        public override int getSupportUpperBound()
        {
            return 0x7fffffff;
        }

        public bool isSupportConnected()
        {
            return true;
        }

        public override double logProbability(int x)
        {
            if (x < 0)
            {
                return double.NegativeInfinity;
            }
            return ((this.binomialCoefficientLog((x + this.numberOfSuccesses) - 1, this.numberOfSuccesses - 1) + (this.logProbabilityOfSuccess * this.numberOfSuccesses)) + (this.log1mProbabilityOfSuccess * x));
        }

        public static int mulAndCheck(int x, int y)
        {
            long num = x * y;
            if ((num < -2147483648L) || (num > 0x7fffffffL))
            {
                throw new ArithmeticException();
            }
            return (int)num;
        }

        public override double probability(int x)
        {
            if (x < 0)
            {
                return 0.0;
            }
            return ((this.binomialCoefficientDouble((x + this.numberOfSuccesses) - 1, this.numberOfSuccesses - 1) * Math.Pow(this.probabilityOfSuccess, (double)this.numberOfSuccesses)) * Math.Pow(1.0 - this.probabilityOfSuccess, (double)x));
        }
    }
}