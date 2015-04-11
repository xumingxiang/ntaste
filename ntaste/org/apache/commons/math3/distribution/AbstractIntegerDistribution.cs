namespace org.apache.commons.math3.distribution
{
    using org.apache.commons.math3.exception;
    using org.apache.commons.math3.random;
    using org.apache.commons.math3.util;
    using System;

    public abstract class AbstractIntegerDistribution
    {
        protected RandomGenerator random;

        protected AbstractIntegerDistribution(RandomGenerator rng)
        {
            this.random = rng;
        }

        private double checkedCumulativeProbability(int argument)
        {
            double naN = double.NaN;
            naN = this.cumulativeProbability(argument);
            if (double.IsNaN(naN))
            {
                throw new Exception("DISCRETE_CUMULATIVE_PROBABILITY_RETURNED_NAN");
            }
            return naN;
        }

        public abstract double cumulativeProbability(int x);

        public double cumulativeProbability(int x0, int x1)
        {
            if (x1 < x0)
            {
                throw new ArgumentException(string.Format("LOWER_ENDPOINT_ABOVE_UPPER_ENDPOINT ({0}, {1})", x0, x1));
            }
            return (this.cumulativeProbability(x1) - this.cumulativeProbability(x0));
        }

        public abstract double getNumericalMean();

        public abstract double getNumericalVariance();

        public abstract int getSupportLowerBound();

        public abstract int getSupportUpperBound();

        public int inverseCumulativeProbability(double p)
        {
            if ((p < 0.0) || (p > 1.0))
            {
                throw new ArgumentOutOfRangeException("p", p, "Should be in (0, 1)");
            }
            int argument = this.getSupportLowerBound();
            if (p == 0.0)
            {
                return argument;
            }
            if (argument == -2147483648)
            {
                if (this.checkedCumulativeProbability(argument) >= p)
                {
                    return argument;
                }
            }
            else
            {
                argument--;
            }
            int upper = this.getSupportUpperBound();
            if (p == 1.0)
            {
                return upper;
            }
            double d = this.getNumericalMean();
            double num4 = Math.Sqrt(this.getNumericalVariance());
            if (((!double.IsInfinity(d) && !double.IsNaN(d)) && (!double.IsInfinity(num4) && !double.IsNaN(num4))) && !(num4 == 0.0))
            {
                double num5 = Math.Sqrt((1.0 - p) / p);
                double a = d - (num5 * num4);
                if (a > argument)
                {
                    argument = ((int)Math.Ceiling(a)) - 1;
                }
                num5 = 1.0 / num5;
                a = d + (num5 * num4);
                if (a < upper)
                {
                    upper = ((int)Math.Ceiling(a)) - 1;
                }
            }
            return this.solveInverseCumulativeProbability(p, argument, upper);
        }

        public virtual double logProbability(int x)
        {
            return MathUtil.Log(this.probability(x));
        }

        public abstract double probability(int x);

        public void reseedRandomGenerator(long seed)
        {
            this.random.setSeed(seed);
        }

        public int sample()
        {
            return this.inverseCumulativeProbability(this.random.nextDouble());
        }

        public int[] sample(int sampleSize)
        {
            if (sampleSize <= 0)
            {
                throw new NotStrictlyPositiveException(sampleSize);
            }
            int[] numArray = new int[sampleSize];
            for (int i = 0; i < sampleSize; i++)
            {
                numArray[i] = this.sample();
            }
            return numArray;
        }

        protected int solveInverseCumulativeProbability(double p, int lower, int upper)
        {
            while ((lower + 1) < upper)
            {
                int argument = (lower + upper) / 2;
                if ((argument < lower) || (argument > upper))
                {
                    argument = lower + ((upper - lower) / 2);
                }
                if (this.checkedCumulativeProbability(argument) >= p)
                {
                    upper = argument;
                }
                else
                {
                    lower = argument;
                }
            }
            return upper;
        }
    }
}