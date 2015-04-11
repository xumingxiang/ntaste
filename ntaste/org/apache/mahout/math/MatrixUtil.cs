namespace org.apache.mahout.math
{
    using System;
    using System.Collections.Generic;

    public static class MatrixUtil
    {
        public static void assign(double[,] m, Func<double, double> f)
        {
            int length = m.GetLength(0);
            int num2 = m.GetLength(1);
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    m[i, j] = f(m[i, j]);
                }
            }
        }

        public static void assign(double[] v, Func<double, double> f)
        {
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = f(v[i]);
            }
        }

        public static IEnumerable<double> nonZeroes(double[] vector)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] != 0.0)
                {
                    yield return vector[i];
                }
            }
        }

        public static double norm1(double[] v)
        {
            double num = 0.0;
            for (int i = 0; i < v.Length; i++)
            {
                num += Math.Abs(v[i]);
            }
            return num;
        }

        public static double norm2(double[] v)
        {
            return Math.Sqrt(vectorDot(v, v));
        }

        public static double[] times(double[] v, double x)
        {
            double[] numArray = new double[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                numArray[i] = v[i] * x;
            }
            return numArray;
        }

        public static double[,] times(double[,] m, double[,] other)
        {
            int length = m.GetLength(1);
            if (length != other.GetLength(0))
            {
                throw new Exception();
            }
            int num2 = m.GetLength(0);
            int num3 = other.GetLength(1);
            double[,] numArray = new double[num2, num3];
            for (int i = 0; i < num2; i++)
            {
                for (int j = 0; j < num3; j++)
                {
                    double num6 = 0.0;
                    for (int k = 0; k < length; k++)
                    {
                        num6 += m[i, k] * other[k, j];
                    }
                    numArray[i, j] = num6;
                }
            }
            return numArray;
        }

        public static double[,] transpose(double[,] m)
        {
            int length = m.GetLength(0);
            int num2 = m.GetLength(1);
            double[,] numArray = new double[num2, length];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < num2; j++)
                {
                    numArray[j, i] = m[i, j];
                }
            }
            return numArray;
        }

        public static double vectorDot(double[] v1, double[] v2)
        {
            double num = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                num += v1[i] * v2[i];
            }
            return num;
        }

        public static double[] viewColumn(double[,] m, int column)
        {
            double[] numArray = new double[m.GetLength(0)];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = m[i, column];
            }
            return numArray;
        }

        public static double[] viewDiagonal(double[,] m)
        {
            double[] numArray = new double[Math.Min(m.GetLength(0), m.GetLength(1))];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = m[i, i];
            }
            return numArray;
        }

        public static double[,] viewPart(double[,] m, int rowOff, int rowRequested, int colOff, int colRequested)
        {
            double[,] numArray = new double[rowRequested - rowOff, colRequested - colOff];
            for (int i = rowOff; i < rowRequested; i++)
            {
                for (int j = colOff; j < colRequested; j++)
                {
                    numArray[i - rowOff, j - colOff] = m[i, j];
                }
            }
            return numArray;
        }

        public static double[] viewRow(double[,] m, int row)
        {
            double[] numArray = new double[m.GetLength(1)];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = m[row, i];
            }
            return numArray;
        }

        public static void WriteVector(string msg, double[] v)
        {
            Console.Write("{0}: ", msg);
            foreach (double num in v)
            {
                Console.Write("{0} ", num);
            }
            Console.WriteLine();
        }
    }
}