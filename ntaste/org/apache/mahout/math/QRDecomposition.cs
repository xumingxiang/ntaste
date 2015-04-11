namespace org.apache.mahout.math
{
    using System;

    public class QRDecomposition
    {
        private int columns;
        private bool fullRank;
        private double[,] q;
        private double[,] r;
        private int rows;

        public QRDecomposition(double[,] a)
        {
            this.rows = a.GetLength(0);
            this.columns = a.GetLength(1);
            int colRequested = Math.Min(a.GetLength(0), a.GetLength(1));
            double[,] m = (double[,])a.Clone();
            bool flag = true;
            this.r = new double[colRequested, this.columns];
            for (int i = 0; i < colRequested; i++)
            {
                int num4;
                double[] v = MatrixUtil.viewColumn(m, i);
                double num3 = MatrixUtil.norm2(v);
                if (Math.Abs(num3) > double.Epsilon)
                {
                    num4 = 0;
                    while (num4 < m.GetLength(0))
                    {
                        double num1 = m[num4, i];
                        v[num4] = num1 /= num3;
                        num4++;
                    }
                }
                else
                {
                    if (double.IsInfinity(num3) || double.IsNaN(num3))
                    {
                        throw new ArithmeticException("Invalid intermediate result");
                    }
                    flag = false;
                }
                this.r[i, i] = num3;
                for (int j = i + 1; j < this.columns; j++)
                {
                    double[] numArray3 = MatrixUtil.viewColumn(m, j);
                    double num6 = MatrixUtil.norm2(numArray3);
                    if (Math.Abs(num6) > double.Epsilon)
                    {
                        double num7 = MatrixUtil.vectorDot(v, numArray3);
                        this.r[i, j] = num7;
                        if (j < colRequested)
                        {
                            for (num4 = 0; num4 < numArray3.Length; num4++)
                            {
                                m[num4, j] = numArray3[num4] += v[num4] * -num7;
                            }
                        }
                    }
                    else if (double.IsInfinity(num6) || double.IsNaN(num6))
                    {
                        throw new ArithmeticException("Invalid intermediate result");
                    }
                }
            }
            if (this.columns > colRequested)
            {
                this.q = MatrixUtil.viewPart(m, 0, this.rows, 0, colRequested);
            }
            else
            {
                this.q = m;
            }
            this.fullRank = flag;
        }

        public double[,] getQ()
        {
            return this.q;
        }

        public double[,] getR()
        {
            return this.r;
        }

        public bool hasFullRank()
        {
            return this.fullRank;
        }

        public double[,] solve(double[,] B)
        {
            if (B.GetLength(0) != this.rows)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            int length = B.GetLength(1);
            double[,] numArray = new double[this.columns, length];
            double[,] numArray3 = MatrixUtil.times(MatrixUtil.transpose(this.getQ()), B);
            double[,] m = this.getR();
            for (int i = Math.Min(this.columns, this.rows) - 1; i >= 0; i--)
            {
                for (int j = 0; j < numArray.GetLength(1); j++)
                {
                    numArray[i, j] += numArray3[i, j] / m[i, i];
                }
                double[] numArray5 = MatrixUtil.viewColumn(m, i);
                for (int k = 0; k < length; k++)
                {
                    for (int n = 0; n < i; n++)
                    {
                        numArray3[n, k] += numArray5[n] * -numArray[i, k];
                    }
                }
            }
            return numArray;
        }

        public override string ToString()
        {
            return string.Format("QR({0} x {1},fullRank={2})", this.rows, this.columns, this.hasFullRank());
        }
    }
}