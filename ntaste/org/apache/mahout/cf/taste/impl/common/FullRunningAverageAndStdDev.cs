namespace org.apache.mahout.cf.taste.impl.common
{
    using System;

    public sealed class FullRunningAverageAndStdDev : FullRunningAverage, RunningAverageAndStdDev, RunningAverage
    {
        private double mk;
        private double sk;
        private double stdDev;

        public FullRunningAverageAndStdDev()
        {
            this.mk = 0.0;
            this.sk = 0.0;
            this.recomputeStdDev();
        }

        public FullRunningAverageAndStdDev(int count, double average, double mk, double sk)
            : base(count, average)
        {
            this.mk = mk;
            this.sk = sk;
            this.recomputeStdDev();
        }

        public override void addDatum(double datum)
        {
            lock (this)
            {
                base.addDatum(datum);
                int num = this.getCount();
                if (num == 1)
                {
                    this.mk = datum;
                    this.sk = 0.0;
                }
                else
                {
                    double mk = this.mk;
                    double num3 = datum - mk;
                    this.mk += num3 / ((double)num);
                    this.sk += num3 * (datum - this.mk);
                }
                this.recomputeStdDev();
            }
        }

        public override void changeDatum(double delta)
        {
            throw new NotSupportedException();
        }

        public double getMk()
        {
            return this.mk;
        }

        public double getSk()
        {
            return this.sk;
        }

        public double getStandardDeviation()
        {
            return this.stdDev;
        }

        public new RunningAverageAndStdDev inverse()
        {
            return new InvertedRunningAverageAndStdDev(this);
        }

        RunningAverage RunningAverage.inverse()
        {
            return new InvertedRunningAverageAndStdDev(this);
        }

        private void recomputeStdDev()
        {
            int num = this.getCount();
            this.stdDev = (num > 1) ? Math.Sqrt(this.sk / ((double)(num - 1))) : double.NaN;
        }

        public override void removeDatum(double datum)
        {
            lock (this)
            {
                int num = this.getCount();
                base.removeDatum(datum);
                double mk = this.mk;
                this.mk = ((num * mk) - datum) / ((double)(num - 1));
                this.sk -= (datum - this.mk) * (datum - mk);
                this.recomputeStdDev();
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", this.getAverage(), this.stdDev);
        }
    }
}