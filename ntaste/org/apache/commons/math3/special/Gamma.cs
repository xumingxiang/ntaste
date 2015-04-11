namespace org.apache.commons.math3.special
{
    using org.apache.commons.math3.util;
    using System;

    public class Gamma
    {
        private static double C_LIMIT = 49.0;
        private static double DEFAULT_EPSILON = 1E-14;
        public const double GAMMA = 0.57721566490153287;
        private static double HALF_LOG_2_PI = (0.5 * MathUtil.Log(6.2831853071795862));
        private static double INV_GAMMA1P_M1_A0 = 6.1160951044814161E-09;
        private static double INV_GAMMA1P_M1_A1 = 6.2473083011646549E-09;
        private static double INV_GAMMA1P_M1_B1 = 0.203610414066807;
        private static double INV_GAMMA1P_M1_B2 = 0.026620534842894922;
        private static double INV_GAMMA1P_M1_B3 = 0.00049394497938244685;
        private static double INV_GAMMA1P_M1_B4 = -8.5141943244031486E-06;
        private static double INV_GAMMA1P_M1_B5 = -6.4304548177935305E-06;
        private static double INV_GAMMA1P_M1_B6 = 9.9264184067277368E-07;
        private static double INV_GAMMA1P_M1_B7 = -6.0776189572282523E-08;
        private static double INV_GAMMA1P_M1_B8 = 1.9575583661463974E-10;
        private static double INV_GAMMA1P_M1_C = -0.42278433509846713;
        private static double INV_GAMMA1P_M1_C0 = 0.57721566490153287;
        private static double INV_GAMMA1P_M1_C1 = -0.6558780715202539;
        private static double INV_GAMMA1P_M1_C10 = -2.0134854780788239E-05;
        private static double INV_GAMMA1P_M1_C11 = -1.2504934821426706E-06;
        private static double INV_GAMMA1P_M1_C12 = 1.1330272319816959E-06;
        private static double INV_GAMMA1P_M1_C13 = -2.0563384169776071E-07;
        private static double INV_GAMMA1P_M1_C2 = -0.042002635034095237;
        private static double INV_GAMMA1P_M1_C3 = 0.16653861138229148;
        private static double INV_GAMMA1P_M1_C4 = -0.042197734555544333;
        private static double INV_GAMMA1P_M1_C5 = -0.009621971527876973;
        private static double INV_GAMMA1P_M1_C6 = 0.0072189432466631;
        private static double INV_GAMMA1P_M1_C7 = -0.0011651675918590652;
        private static double INV_GAMMA1P_M1_C8 = -0.00021524167411495098;
        private static double INV_GAMMA1P_M1_C9 = 0.0001280502823881162;
        private static double INV_GAMMA1P_M1_P0 = 6.1160951044814161E-09;
        private static double INV_GAMMA1P_M1_P1 = 6.8716741130671986E-09;
        private static double INV_GAMMA1P_M1_P2 = 6.8201616684961707E-10;
        private static double INV_GAMMA1P_M1_P3 = 4.6868433229488481E-11;
        private static double INV_GAMMA1P_M1_P4 = 1.5728330277104463E-12;
        private static double INV_GAMMA1P_M1_P5 = -1.2494415722763663E-13;
        private static double INV_GAMMA1P_M1_P6 = 4.3435299374085941E-15;
        private static double INV_GAMMA1P_M1_Q1 = 0.30569610783652212;
        private static double INV_GAMMA1P_M1_Q2 = 0.054642130860422966;
        private static double INV_GAMMA1P_M1_Q3 = 0.0049568300938258869;
        private static double INV_GAMMA1P_M1_Q4 = 0.00026923694661863613;
        private static double[] LANCZOS = new double[] { 0.99999999999999711, 57.156235665862923, -59.597960355475493, 14.136097974741746, -0.49191381609762019, 3.3994649984811891E-05, 4.6523628927048578E-05, -9.8374475304879565E-05, 0.00015808870322491249, -0.00021026444172410488, 0.00021743961811521265, -0.00016431810653676389, 8.441822398385275E-05, -2.6190838401581408E-05, 3.6899182659531625E-06 };
        public const double LANCZOS_G = 4.7421875;
        private static double S_LIMIT = 1E-05;
        private static double SQRT_TWO_PI = 2.5066282746310007;

        private Gamma()
        {
        }

        public static double digamma(double x)
        {
            if ((x > 0.0) && (x <= S_LIMIT))
            {
                return (-0.57721566490153287 - (1.0 / x));
            }
            if (x >= C_LIMIT)
            {
                double num = 1.0 / (x * x);
                return ((MathUtil.Log(x) - (0.5 / x)) - (num * (0.083333333333333329 + (num * (0.0083333333333333332 - (num / 252.0))))));
            }
            return (digamma(x + 1.0) - (1.0 / x));
        }

        public static double gamma(double x)
        {
            double num;
            if ((x == rint(x)) && (x <= 0.0))
            {
                return double.NaN;
            }
            double num2 = Math.Abs(x);
            if (num2 <= 20.0)
            {
                double num3;
                double num4;
                if (x >= 1.0)
                {
                    num3 = 1.0;
                    num4 = x;
                    while (num4 > 2.5)
                    {
                        num4--;
                        num3 *= num4;
                    }
                    return (num3 / (1.0 + invGamma1pm1(num4 - 1.0)));
                }
                num3 = x;
                num4 = x;
                while (num4 < -0.5)
                {
                    num4++;
                    num3 *= num4;
                }
                return (1.0 / (num3 * (1.0 + invGamma1pm1(num4))));
            }
            double num5 = (num2 + 4.7421875) + 0.5;
            double num6 = (((SQRT_TWO_PI / x) * Math.Pow(num5, num2 + 0.5)) * Math.Exp(-num5)) * lanczos(num2);
            if (x > 0.0)
            {
                num = num6;
            }
            else
            {
                num = -3.1415926535897931 / ((x * Math.Sin(3.1415926535897931 * x)) * num6);
            }
            return num;
        }

        public static double invGamma1pm1(double x)
        {
            double num5;
            if (x < -0.5)
            {
                throw new ArgumentException();
            }
            if (x > 1.5)
            {
                throw new ArgumentException();
            }
            double num2 = (x <= 0.5) ? x : ((x - 0.5) - 0.5);
            if (num2 < 0.0)
            {
                double num3 = INV_GAMMA1P_M1_A0 + (num2 * INV_GAMMA1P_M1_A1);
                double num4 = INV_GAMMA1P_M1_B8;
                num4 = INV_GAMMA1P_M1_B7 + (num2 * num4);
                num4 = INV_GAMMA1P_M1_B6 + (num2 * num4);
                num4 = INV_GAMMA1P_M1_B5 + (num2 * num4);
                num4 = INV_GAMMA1P_M1_B4 + (num2 * num4);
                num4 = INV_GAMMA1P_M1_B3 + (num2 * num4);
                num4 = INV_GAMMA1P_M1_B2 + (num2 * num4);
                num4 = INV_GAMMA1P_M1_B1 + (num2 * num4);
                num4 = 1.0 + (num2 * num4);
                num5 = INV_GAMMA1P_M1_C13 + (num2 * (num3 / num4));
                num5 = INV_GAMMA1P_M1_C12 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C11 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C10 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C9 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C8 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C7 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C6 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C5 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C4 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C3 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C2 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C1 + (num2 * num5);
                num5 = INV_GAMMA1P_M1_C + (num2 * num5);
                if (x > 0.5)
                {
                    return ((num2 * num5) / x);
                }
                return (x * ((num5 + 0.5) + 0.5));
            }
            double num6 = INV_GAMMA1P_M1_P6;
            num6 = INV_GAMMA1P_M1_P5 + (num2 * num6);
            num6 = INV_GAMMA1P_M1_P4 + (num2 * num6);
            num6 = INV_GAMMA1P_M1_P3 + (num2 * num6);
            num6 = INV_GAMMA1P_M1_P2 + (num2 * num6);
            num6 = INV_GAMMA1P_M1_P1 + (num2 * num6);
            num6 = INV_GAMMA1P_M1_P0 + (num2 * num6);
            double num7 = INV_GAMMA1P_M1_Q4;
            num7 = INV_GAMMA1P_M1_Q3 + (num2 * num7);
            num7 = INV_GAMMA1P_M1_Q2 + (num2 * num7);
            num7 = INV_GAMMA1P_M1_Q1 + (num2 * num7);
            num7 = 1.0 + (num2 * num7);
            num5 = INV_GAMMA1P_M1_C13 + ((num6 / num7) * num2);
            num5 = INV_GAMMA1P_M1_C12 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C11 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C10 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C9 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C8 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C7 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C6 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C5 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C4 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C3 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C2 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C1 + (num2 * num5);
            num5 = INV_GAMMA1P_M1_C0 + (num2 * num5);
            if (x > 0.5)
            {
                return ((num2 / x) * ((num5 - 0.5) - 0.5));
            }
            return (x * num5);
        }

        public static double lanczos(double x)
        {
            double num = 0.0;
            for (int i = LANCZOS.Length - 1; i > 0; i--)
            {
                num += LANCZOS[i] / (x + i);
            }
            return (num + LANCZOS[0]);
        }

        public static double logGamma(double x)
        {
            if (double.IsNaN(x) || (x <= 0.0))
            {
                return double.NaN;
            }
            if (x < 0.5)
            {
                return (logGamma1p(x) - MathUtil.Log(x));
            }
            if (x <= 2.5)
            {
                return logGamma1p((x - 0.5) - 0.5);
            }
            if (x <= 8.0)
            {
                int num2 = (int)Math.Floor((double)(x - 1.5));
                double num3 = 1.0;
                for (int i = 1; i <= num2; i++)
                {
                    num3 *= x - i;
                }
                return (logGamma1p(x - (num2 + 1)) + MathUtil.Log(num3));
            }
            double num5 = lanczos(x);
            double num6 = (x + 4.7421875) + 0.5;
            return (((((x + 0.5) * MathUtil.Log(num6)) - num6) + HALF_LOG_2_PI) + MathUtil.Log(num5 / x));
        }

        public static double logGamma1p(double x)
        {
            if (x < -0.5)
            {
                throw new ArgumentException();
            }
            if (x > 1.5)
            {
                throw new ArgumentException();
            }
            return -MathUtil.Log1p(invGamma1pm1(x));
        }

        public static double regularizedGammaP(double a, double x)
        {
            return regularizedGammaP(a, x, DEFAULT_EPSILON, 0x7fffffff);
        }

        public static double regularizedGammaP(double a, double x, double epsilon, int maxIterations)
        {
            if (((double.IsNaN(a) || double.IsNaN(x)) || (a <= 0.0)) || (x < 0.0))
            {
                return double.NaN;
            }
            if (x == 0.0)
            {
                return 0.0;
            }
            if (x >= (a + 1.0))
            {
                return (1.0 - regularizedGammaQ(a, x, epsilon, maxIterations));
            }
            double num2 = 0.0;
            double num3 = 1.0 / a;
            double d = num3;
            while (((Math.Abs((double)(num3 / d)) > epsilon) && (num2 < maxIterations)) && (d < double.PositiveInfinity))
            {
                num2++;
                num3 *= x / (a + num2);
                d += num3;
            }
            if (num2 >= maxIterations)
            {
                throw new ArithmeticException("MaxCountExceededException");
            }
            if (double.IsInfinity(d))
            {
                return 1.0;
            }
            return (Math.Exp((-x + (a * MathUtil.Log(x))) - logGamma(a)) * d);
        }

        public static double regularizedGammaQ(double a, double x)
        {
            return regularizedGammaQ(a, x, DEFAULT_EPSILON, 0x7fffffff);
        }

        public static double regularizedGammaQ(double a, double x, double epsilon, int maxIterations)
        {
            if (((double.IsNaN(a) || double.IsNaN(x)) || (a <= 0.0)) || (x < 0.0))
            {
                return double.NaN;
            }
            if (x == 0.0)
            {
                return 1.0;
            }
            if (x < (a + 1.0))
            {
                return (1.0 - regularizedGammaP(a, x, epsilon, maxIterations));
            }
            ContinuedFraction fraction = new GammaContinuedFraction(a);
            double num = 1.0 / fraction.evaluate(x, epsilon, maxIterations);
            return (Math.Exp((-x + (a * MathUtil.Log(x))) - logGamma(a)) * num);
        }

        public static double rint(double x)
        {
            double num = Math.Floor(x);
            double num2 = x - num;
            if (num2 > 0.5)
            {
                if (num == -1.0)
                {
                    return 0.0;
                }
                return (num + 1.0);
            }
            if (num2 < 0.5)
            {
                return num;
            }
            long num3 = (long)num;
            return (((num3 & 1L) == 0L) ? num : (num + 1.0));
        }

        public static double trigamma(double x)
        {
            if ((x > 0.0) && (x <= S_LIMIT))
            {
                return (1.0 / (x * x));
            }
            if (x >= C_LIMIT)
            {
                double num = 1.0 / (x * x);
                return (((1.0 / x) + (num / 2.0)) + ((num / x) * (0.16666666666666666 - (num * (0.033333333333333333 + (num / 42.0))))));
            }
            return (trigamma(x + 1.0) + (1.0 / (x * x)));
        }

        private class GammaContinuedFraction : ContinuedFraction
        {
            private double a;

            internal GammaContinuedFraction(double a)
            {
                this.a = a;
            }

            protected override double getA(int n, double x)
            {
                return ((((2.0 * n) + 1.0) - this.a) + x);
            }

            protected override double getB(int n, double x)
            {
                return (n * (this.a - n));
            }
        }
    }
}