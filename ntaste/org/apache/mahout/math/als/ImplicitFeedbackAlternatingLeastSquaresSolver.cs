namespace org.apache.mahout.math.als
{
    using org.apache.mahout.math;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ImplicitFeedbackAlternatingLeastSquaresSolver
    {
        private double alpha;
        private double lambda;
        private int numFeatures;
        private IDictionary<int, double[]> Y;
        private double[,] YtransposeY;

        public ImplicitFeedbackAlternatingLeastSquaresSolver(int numFeatures, double lambda, double alpha, IDictionary<int, double[]> Y)
        {
            this.numFeatures = numFeatures;
            this.lambda = lambda;
            this.alpha = alpha;
            this.Y = Y;
            this.YtransposeY = this.getYtransposeY(Y);
        }

        private double[,] columnVectorAsMatrix(double[] v)
        {
            double[,] numArray = new double[this.numFeatures, 1];
            for (int i = 0; i < v.Length; i++)
            {
                numArray[i, 0] = v[i];
            }
            return numArray;
        }

        private double confidence(double rating)
        {
            return (1.0 + (this.alpha * rating));
        }

        private double[,] getYtransponseCuMinusIYPlusLambdaI(IList<Tuple<int, double>> userRatings)
        {
            Dictionary<int, double[]> dictionary = new Dictionary<int, double[]>(userRatings.Count);
            foreach (Tuple<int, double> tuple in userRatings)
            {
                dictionary[tuple.Item1] = MatrixUtil.times(this.Y[tuple.Item1], this.confidence(tuple.Item2) - 1.0);
            }
            double[,] numArray = new double[this.numFeatures, this.numFeatures];
            foreach (Tuple<int, double> tuple in userRatings)
            {
                int num = tuple.Item1;
                int num2 = 0;
                foreach (double num3 in this.Y[num])
                {
                    int num4 = num2++;
                    double[] numArray2 = MatrixUtil.times(dictionary[num], num3);
                    for (int j = 0; j < numArray.GetLength(1); j++)
                    {
                        double num1 = numArray[num4, j];
                        num1 += numArray2[j];
                    }
                }
            }
            for (int i = 0; i < this.numFeatures; i++)
            {
                numArray[i, i] += this.lambda;
            }
            return numArray;
        }

        private double[,] getYtransponseCuPu(IList<Tuple<int, double>> userRatings)
        {
            double[] v = new double[this.numFeatures];
            foreach (Tuple<int, double> tuple in userRatings)
            {
                double[] numArray2 = MatrixUtil.times(this.Y[tuple.Item1], this.confidence(tuple.Item2));
                for (int i = 0; i < v.Length; i++)
                {
                    v[i] += numArray2[i];
                }
            }
            return this.columnVectorAsMatrix(v);
        }

        private double[,] getYtransposeY(IDictionary<int, double[]> Y)
        {
            List<int> list = Y.Keys.ToList<int>();
            list.Sort();
            int count = list.Count;
            double[,] numArray = new double[this.numFeatures, this.numFeatures];
            for (int i = 0; i < this.numFeatures; i++)
            {
                for (int j = i; j < this.numFeatures; j++)
                {
                    double num4 = 0.0;
                    for (int k = 0; k < count; k++)
                    {
                        double[] numArray2 = Y[list[k]];
                        num4 += numArray2[i] * numArray2[j];
                    }
                    numArray[i, j] = num4;
                    if (i != j)
                    {
                        numArray[j, i] = num4;
                    }
                }
            }
            return numArray;
        }

        public double[] solve(IList<Tuple<int, double>> ratings)
        {
            double[,] numArray = this.getYtransponseCuMinusIYPlusLambdaI(ratings);
            double[,] a = new double[this.YtransposeY.GetLength(0), this.YtransposeY.GetLength(1)];
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    a[i, j] = this.YtransposeY[i, j] + numArray[i, j];
                }
            }
            return solve(a, this.getYtransponseCuPu(ratings));
        }

        private static double[] solve(double[,] A, double[,] y)
        {
            return MatrixUtil.viewColumn(new QRDecomposition(A).solve(y), 0);
        }
    }
}