namespace org.apache.commons.math3.util
{
    using System;

    public abstract class ContinuedFraction
    {
        private static double DEFAULT_EPSILON = 1E-08;

        protected ContinuedFraction()
        {
        }

        public double evaluate(double x)
        {
            return this.evaluate(x, DEFAULT_EPSILON, 0x7fffffff);
        }

        public double evaluate(double x, double epsilon)
        {
            return this.evaluate(x, epsilon, 0x7fffffff);
        }

        public double evaluate(double x, int maxIterations)
        {
            return this.evaluate(x, DEFAULT_EPSILON, maxIterations);
        }

        public double evaluate(double x, double epsilon, int maxIterations)
        {
            double eps = 1E-50;
            double num2 = this.getA(0, x);
            if (this.PrecisionEquals(num2, 0.0, eps))
            {
                num2 = eps;
            }
            int n = 1;
            double num4 = 0.0;
            double num5 = num2;
            double d = num2;
            while (n < maxIterations)
            {
                double num7 = this.getA(n, x);
                double num8 = this.getB(n, x);
                double num9 = num7 + (num8 * num4);
                if (this.PrecisionEquals(num9, 0.0, eps))
                {
                    num9 = eps;
                }
                double num10 = num7 + (num8 / num5);
                if (this.PrecisionEquals(num10, 0.0, eps))
                {
                    num10 = eps;
                }
                num9 = 1.0 / num9;
                double num11 = num10 * num9;
                d = num2 * num11;
                if (double.IsInfinity(d))
                {
                    throw new OverflowException(string.Format("CONTINUED_FRACTION_INFINITY_DIVERGENCE {0}", x));
                }
                if (double.IsNaN(d))
                {
                    throw new OverflowException(string.Format("CONTINUED_FRACTION_NAN_DIVERGENCE {0}", x));
                }
                if (Math.Abs((double)(num11 - 1.0)) < epsilon)
                {
                    break;
                }
                num4 = num9;
                num5 = num10;
                num2 = d;
                n++;
            }
            if (n >= maxIterations)
            {
                throw new Exception(string.Format("NON_CONVERGENT_CONTINUED_FRACTION iter={0} x={1}", maxIterations, x));
            }
            return d;
        }

        protected abstract double getA(int n, double x);

        protected abstract double getB(int n, double x);

        protected bool PrecisionEquals(double x, double y, double eps)
        {
            return ((x == y) || (Math.Abs((double)(y - x)) <= eps));
        }
    }
}