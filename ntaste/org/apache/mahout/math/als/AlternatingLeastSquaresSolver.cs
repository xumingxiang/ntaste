namespace org.apache.mahout.math.als
{
    using org.apache.mahout.math;
    using System.Collections.Generic;

    public sealed class AlternatingLeastSquaresSolver
    {
        private AlternatingLeastSquaresSolver()
        {
        }

        public static double[,] addLambdaTimesNuiTimesE(double[,] matrix, double lambda, int nui)
        {
            double num = lambda * nui;
            int length = matrix.GetLength(1);
            for (int i = 0; i < length; i++)
            {
                double num1 = matrix[i, i];
                num1 += matrix[i, i] + num;
            }
            return matrix;
        }

        public static double[,] createMiIi(IList<double[]> featureVectors, int numFeatures)
        {
            double[,] numArray = new double[numFeatures, featureVectors.Count];
            int num = 0;
            foreach (double[] numArray2 in featureVectors)
            {
                for (int i = 0; i < numFeatures; i++)
                {
                    numArray[i, num] = numArray2[i];
                }
                num++;
            }
            return numArray;
        }

        public static double[,] createRiIiMaybeTransposed(double[] ratingVector)
        {
            double[,] numArray = new double[ratingVector.Length, 1];
            int num = 0;
            foreach (double num2 in MatrixUtil.nonZeroes(ratingVector))
            {
                numArray[num++, 0] = num2;
            }
            return numArray;
        }

        private static double[,] miTimesMiTransposePlusLambdaTimesNuiTimesE(double[,] MiIi, double lambda, int nui)
        {
            double num = lambda * nui;
            int length = MiIi.GetLength(0);
            double[,] numArray = new double[length, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = i; j < length; j++)
                {
                    double num5 = MatrixUtil.vectorDot(MatrixUtil.viewRow(MiIi, i), MatrixUtil.viewRow(MiIi, j));
                    if (i != j)
                    {
                        numArray[i, j] = num5;
                        numArray[j, i] = num5;
                    }
                    else
                    {
                        numArray[i, i] = num5 + num;
                    }
                }
            }
            return numArray;
        }

        private static double[] solve(double[,] Ai, double[,] Vi)
        {
            return MatrixUtil.viewColumn(new QRDecomposition(Ai).solve(Vi), 0);
        }

        public static double[] solve(IList<double[]> featureVectors, double[] ratingVector, double lambda, int numFeatures)
        {
            int length = ratingVector.Length;
            double[,] miIi = createMiIi(featureVectors, numFeatures);
            double[,] other = createRiIiMaybeTransposed(ratingVector);
            double[,] ai = miTimesMiTransposePlusLambdaTimesNuiTimesE(miIi, lambda, length);
            double[,] vi = MatrixUtil.times(miIi, other);
            return solve(ai, vi);
        }
    }
}