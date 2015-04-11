using System;

namespace org.apache.mahout.cf.taste.impl.recommender.knn
{
    public sealed class NonNegativeQuadraticOptimizer : Optimizer
    {
        private static double EPSILON = 1.0e-10;
        private static double CONVERGENCE_LIMIT = 0.1;
        private static int MAX_ITERATIONS = 1000;
        private static double DEFAULT_STEP = 0.001;

        /**
         * Non-negative Quadratic Optimization.
         *
         * @param matrix
         *          matrix nxn positions
         * @param b
         *          vector b, n positions
         * @return vector of n weights
         */

        public double[] optimize(double[][] matrix, double[] b)
        {
            int k = b.Length;
            double[] r = new double[k];
            double[] x = new double[k];

            //Arrays.fill(x, 3.0 / k);

            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 3.0 / k;
            }

            for (int iteration = 0; iteration < MAX_ITERATIONS; iteration++)
            {
                double rdot = 0.0;
                for (int n = 0; n < k; n++)
                {
                    double sumAw = 0.0;
                    double[] rowAn = matrix[n];
                    for (int i = 0; i < k; i++)
                    {
                        sumAw += rowAn[i] * x[i];
                    }
                    // r = b - Ax; // the residual, or 'steepest gradient'
                    double rn = b[n] - sumAw;

                    // find active variables - those that are pinned due to
                    // nonnegativity constraints; set respective ri's to zero
                    if (x[n] < EPSILON && rn < 0.0)
                    {
                        rn = 0.0;
                    }
                    else
                    {
                        // max step size numerator
                        rdot += rn * rn;
                    }
                    r[n] = rn;
                }

                if (rdot <= CONVERGENCE_LIMIT)
                {
                    break;
                }

                // max step size denominator
                double rArdotSum = 0.0;
                for (int n = 0; n < k; n++)
                {
                    double sumAr = 0.0;
                    double[] rowAn = matrix[n];
                    for (int i = 0; i < k; i++)
                    {
                        sumAr += rowAn[i] * r[i];
                    }
                    rArdotSum += r[n] * sumAr;
                }

                // max step size
                double stepSize = rdot / rArdotSum;

                if (Double.IsNaN(stepSize))
                {
                    stepSize = DEFAULT_STEP;
                }

                // adjust step size to prevent negative values
                for (int n = 0; n < k; n++)
                {
                    if (r[n] < 0.0)
                    {
                        double absStepSize = stepSize < 0.0 ? -stepSize : stepSize;
                        stepSize = Math.Min(absStepSize, Math.Abs(x[n] / r[n])) * stepSize / absStepSize;
                    }
                }

                // update x values
                for (int n = 0; n < k; n++)
                {
                    x[n] += stepSize * r[n];
                    if (x[n] < EPSILON)
                    {
                        x[n] = 0.0;
                    }
                }

                // TODO: do something in case of divergence
            }

            return x;
        }
    }
}