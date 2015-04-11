namespace org.apache.commons.math3.special
{
    using org.apache.commons.math3.util;
    using System;

    public class Beta
    {
        private static double DEFAULT_EPSILON = 1E-14;
        private static double[] DELTA = new double[] { 0.083333333333333329, -2.7777777777777779E-05, 7.9365079365079371E-08, -5.9523809523809525E-10, 8.417508417508329E-12, -1.9175269175185461E-13, 6.4102564051032548E-15, -2.9550651412533822E-16, 1.7964371635940225E-17, -1.3922896466162779E-18, 1.338028550140209E-19, -1.5424600986796609E-20, 1.9770199298095743E-21, -2.3406566479399704E-22, 1.7134801496639859E-23 };
        private static double HALF_LOG_TWO_PI = 0.91893853320467267;

        private Beta()
        {
        }

        private static double deltaMinusDeltaSum(double a, double b)
        {
            int num5;
            if ((a < 0.0) || (a > b))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (b < 10.0)
            {
                throw new ArgumentException();
            }
            double num = a / b;
            double num2 = num / (1.0 + num);
            double num3 = 1.0 / (1.0 + num);
            double num4 = num3 * num3;
            double[] numArray = new double[DELTA.Length];
            numArray[0] = 1.0;
            for (num5 = 1; num5 < numArray.Length; num5++)
            {
                numArray[num5] = 1.0 + (num3 + (num4 * numArray[num5 - 1]));
            }
            double num6 = 10.0 / b;
            double num7 = num6 * num6;
            double num8 = DELTA[DELTA.Length - 1] * numArray[numArray.Length - 1];
            for (num5 = DELTA.Length - 2; num5 >= 0; num5--)
            {
                num8 = (num7 * num8) + (DELTA[num5] * numArray[num5]);
            }
            return ((num8 * num2) / b);
        }

        public static double logBeta(double p, double q)
        {
            double num4;
            double num9;
            double num14;
            if (((double.IsNaN(p) || double.IsNaN(q)) || (p <= 0.0)) || (q <= 0.0))
            {
                return double.NaN;
            }
            double num = Math.Min(p, q);
            double num2 = Math.Max(p, q);
            if (num >= 10.0)
            {
                double num3 = sumDeltaMinusDeltaSum(num, num2);
                num4 = num / num2;
                double x = num4 / (1.0 + num4);
                double num6 = -(num - 0.5) * MathUtil.Log(x);
                double num7 = num2 * MathUtil.Log1p(num4);
                if (num6 <= num7)
                {
                    return (((((-0.5 * MathUtil.Log(num2)) + HALF_LOG_TWO_PI) + num3) - num6) - num7);
                }
                return (((((-0.5 * MathUtil.Log(num2)) + HALF_LOG_TWO_PI) + num3) - num7) - num6);
            }
            if (num > 2.0)
            {
                double num10;
                if (num2 > 1000.0)
                {
                    int num8 = (int)Math.Floor((double)(num - 1.0));
                    num9 = 1.0;
                    num10 = num;
                    for (int i = 0; i < num8; i++)
                    {
                        num10--;
                        num9 *= num10 / (1.0 + (num10 / num2));
                    }
                    return ((MathUtil.Log(num9) - (num8 * MathUtil.Log(num2))) + (Gamma.logGamma(num10) + logGammaMinusLogGammaSum(num10, num2)));
                }
                double num12 = 1.0;
                num10 = num;
                while (num10 > 2.0)
                {
                    num10--;
                    num4 = num10 / num2;
                    num12 *= num4 / (1.0 + num4);
                }
                if (num2 < 10.0)
                {
                    double num13 = 1.0;
                    num14 = num2;
                    while (num14 > 2.0)
                    {
                        num14--;
                        num13 *= num14 / (num10 + num14);
                    }
                    return ((MathUtil.Log(num12) + MathUtil.Log(num13)) + (Gamma.logGamma(num10) + (Gamma.logGamma(num14) - logGammaSum(num10, num14))));
                }
                return ((MathUtil.Log(num12) + Gamma.logGamma(num10)) + logGammaMinusLogGammaSum(num10, num2));
            }
            if (num >= 1.0)
            {
                if (num2 <= 2.0)
                {
                    return ((Gamma.logGamma(num) + Gamma.logGamma(num2)) - logGammaSum(num, num2));
                }
                if (num2 < 10.0)
                {
                    num9 = 1.0;
                    num14 = num2;
                    while (num14 > 2.0)
                    {
                        num14--;
                        num9 *= num14 / (num + num14);
                    }
                    return (MathUtil.Log(num9) + (Gamma.logGamma(num) + (Gamma.logGamma(num14) - logGammaSum(num, num14))));
                }
                return (Gamma.logGamma(num) + logGammaMinusLogGammaSum(num, num2));
            }
            if (num2 >= 10.0)
            {
                return (Gamma.logGamma(num) + logGammaMinusLogGammaSum(num, num2));
            }
            return MathUtil.Log((Gamma.gamma(num) * Gamma.gamma(num2)) / Gamma.gamma(num + num2));
        }

        public static double logBeta(double a, double b, double epsilon, int maxIterations)
        {
            return logBeta(a, b);
        }

        private static double logGammaMinusLogGammaSum(double a, double b)
        {
            double num;
            double num2;
            if (a < 0.0)
            {
                throw new ArgumentException("NumberIsTooSmall", "a");
            }
            if (b < 10.0)
            {
                throw new ArgumentException("NumberIsTooSmall", "b");
            }
            if (a <= b)
            {
                num = b + (a - 0.5);
                num2 = deltaMinusDeltaSum(a, b);
            }
            else
            {
                num = a + (b - 0.5);
                num2 = deltaMinusDeltaSum(b, a);
            }
            double num3 = num * MathUtil.Log1p(a / b);
            double num4 = a * (MathUtil.Log(b) - 1.0);
            return ((num3 <= num4) ? ((num2 - num3) - num4) : ((num2 - num4) - num3));
        }

        private static double logGammaSum(double a, double b)
        {
            if ((a < 1.0) || (a > 2.0))
            {
                throw new ArgumentOutOfRangeException("Out of range");
            }
            if ((b < 1.0) || (b > 2.0))
            {
                throw new ArgumentOutOfRangeException("Out of range");
            }
            double x = (a - 1.0) + (b - 1.0);
            if (x <= 0.5)
            {
                return Gamma.logGamma1p(1.0 + x);
            }
            if (x <= 1.5)
            {
                return (Gamma.logGamma1p(x) + MathUtil.Log1p(x));
            }
            return (Gamma.logGamma1p(x - 1.0) + MathUtil.Log(x * (1.0 + x)));
        }

        public static double regularizedBeta(double x, double a, double b)
        {
            return regularizedBeta(x, a, b, DEFAULT_EPSILON, 0x7fffffff);
        }

        public static double regularizedBeta(double x, double a, double b, double epsilon)
        {
            return regularizedBeta(x, a, b, epsilon, 0x7fffffff);
        }

        public static double regularizedBeta(double x, double a, double b, int maxIterations)
        {
            return regularizedBeta(x, a, b, DEFAULT_EPSILON, maxIterations);
        }

        public static double regularizedBeta(double x, double a, double b, double epsilon, int maxIterations)
        {
            if ((((double.IsNaN(x) || double.IsNaN(a)) || (double.IsNaN(b) || (x < 0.0))) || ((x > 1.0) || (a <= 0.0))) || (b <= 0.0))
            {
                return double.NaN;
            }
            if ((x > ((a + 1.0) / ((2.0 + b) + a))) && ((1.0 - x) <= ((b + 1.0) / ((2.0 + b) + a))))
            {
                return (1.0 - regularizedBeta(1.0 - x, b, a, epsilon, maxIterations));
            }
            ContinuedFraction fraction = new BetaContinuedFraction(a, b);
            return ((Math.Exp((((a * MathUtil.Log(x)) + (b * MathUtil.Log1p(-x))) - MathUtil.Log(a)) - logBeta(a, b)) * 1.0) / fraction.evaluate(x, epsilon, maxIterations));
        }

        private static double sumDeltaMinusDeltaSum(double p, double q)
        {
            if (p < 10.0)
            {
                throw new ArgumentException();
            }
            if (q < 10.0)
            {
                throw new ArgumentException();
            }
            double a = Math.Min(p, q);
            double b = Math.Max(p, q);
            double num3 = 10.0 / a;
            double num4 = num3 * num3;
            double num5 = DELTA[DELTA.Length - 1];
            for (int i = DELTA.Length - 2; i >= 0; i--)
            {
                num5 = (num4 * num5) + DELTA[i];
            }
            return ((num5 / a) + deltaMinusDeltaSum(a, b));
        }

        private class BetaContinuedFraction : ContinuedFraction
        {
            private double a;
            private double b;

            internal BetaContinuedFraction(double a, double b)
            {
                this.a = a;
                this.b = b;
            }

            protected override double getA(int n, double x)
            {
                return 1.0;
            }

            protected override double getB(int n, double x)
            {
                double num2;
                if ((n % 2) == 0)
                {
                    num2 = ((double)n) / 2.0;
                    return (((num2 * (this.b - num2)) * x) / (((this.a + (2.0 * num2)) - 1.0) * (this.a + (2.0 * num2))));
                }
                num2 = (n - 1.0) / 2.0;
                return (-(((this.a + num2) * ((this.a + this.b) + num2)) * x) / ((this.a + (2.0 * num2)) * ((this.a + (2.0 * num2)) + 1.0)));
            }
        }
    }
}