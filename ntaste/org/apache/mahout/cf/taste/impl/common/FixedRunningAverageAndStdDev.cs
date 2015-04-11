namespace org.apache.mahout.cf.taste.impl.common
{
    public sealed class FixedRunningAverageAndStdDev : FixedRunningAverage, RunningAverageAndStdDev, RunningAverage
    {
        private double stdDev;

        public FixedRunningAverageAndStdDev(double average, double stdDev, int count)
            : base(average, count)
        {
            this.stdDev = stdDev;
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

        public override string ToString()
        {
            return (base.ToString() + ',' + this.stdDev.ToString());
        }
    }
}