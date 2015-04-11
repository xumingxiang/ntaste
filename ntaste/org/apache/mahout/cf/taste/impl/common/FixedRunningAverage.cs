namespace org.apache.mahout.cf.taste.impl.common
{
    using System;

    public class FixedRunningAverage : RunningAverage
    {
        private double average;
        private int count;

        public FixedRunningAverage(double average, int count)
        {
            this.average = average;
            this.count = count;
        }

        public void addDatum(double datum)
        {
            throw new NotSupportedException();
        }

        public void changeDatum(double delta)
        {
            throw new NotSupportedException();
        }

        public double getAverage()
        {
            return this.average;
        }

        public int getCount()
        {
            return this.count;
        }

        public RunningAverage inverse()
        {
            return new InvertedRunningAverage(this);
        }

        public void removeDatum(double datum)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return this.average.ToString();
        }
    }
}