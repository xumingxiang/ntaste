namespace org.apache.mahout.cf.taste.impl.common
{
    using System;

    public sealed class WeightedRunningAverageAndStdDev : WeightedRunningAverage, RunningAverageAndStdDev, RunningAverage
    {
        private double totalSquaredWeight = 0.0;
        private double totalWeightedData = 0.0;
        private double totalWeightedSquaredData = 0.0;

        public override void addDatum(double datum, double weight)
        {
            lock (this)
            {
                base.addDatum(datum, weight);
                this.totalSquaredWeight += weight * weight;
                double num = datum * weight;
                this.totalWeightedData += num;
                this.totalWeightedSquaredData += num * datum;
            }
        }

        public void changeDatum(double delta, double weight)
        {
            throw new NotSupportedException();
        }

        public double getStandardDeviation()
        {
            double num = this.getTotalWeight();
            return Math.Sqrt(((this.totalWeightedSquaredData * num) - (this.totalWeightedData * this.totalWeightedData)) / ((num * num) - this.totalSquaredWeight));
        }

        public new RunningAverageAndStdDev inverse()
        {
            return new InvertedRunningAverageAndStdDev(this);
        }

        RunningAverage RunningAverage.inverse()
        {
            return new InvertedRunningAverageAndStdDev(this);
        }

        public override void removeDatum(double datum, double weight)
        {
            lock (this)
            {
                base.removeDatum(datum, weight);
                this.totalSquaredWeight -= weight * weight;
                if (this.totalSquaredWeight <= 0.0)
                {
                    this.totalSquaredWeight = 0.0;
                }
                double num = datum * weight;
                this.totalWeightedData -= num;
                if (this.totalWeightedData <= 0.0)
                {
                    this.totalWeightedData = 0.0;
                }
                this.totalWeightedSquaredData -= num * datum;
                if (this.totalWeightedSquaredData <= 0.0)
                {
                    this.totalWeightedSquaredData = 0.0;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", this.getAverage(), this.getStandardDeviation());
        }
    }
}