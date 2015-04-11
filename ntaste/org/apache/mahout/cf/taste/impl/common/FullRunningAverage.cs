namespace org.apache.mahout.cf.taste.impl.common
{
    using System;

    public class FullRunningAverage : RunningAverage
    {
        private double average;
        private int count;

        public FullRunningAverage()
            : this(0, double.NaN)
        {
        }

        public FullRunningAverage(int count, double average)
        {
            this.count = count;
            this.average = average;
        }

        public virtual void addDatum(double datum)
        {
            if (++this.count == 1)
            {
                this.average = datum;
            }
            else
            {
                this.average = ((this.average * (this.count - 1)) / ((double)this.count)) + (datum / ((double)this.count));
            }
        }

        public virtual void changeDatum(double delta)
        {
            if (this.count == 0)
            {
                throw new InvalidOperationException();
            }
            this.average += delta / ((double)this.count);
        }

        public virtual double getAverage()
        {
            return this.average;
        }

        public virtual int getCount()
        {
            return this.count;
        }

        public RunningAverage inverse()
        {
            return new InvertedRunningAverage(this);
        }

        public virtual void removeDatum(double datum)
        {
            if (this.count == 0)
            {
                throw new InvalidOperationException();
            }
            if (--this.count == 0)
            {
                this.average = double.NaN;
            }
            else
            {
                this.average = ((this.average * (this.count + 1)) / ((double)this.count)) - (datum / ((double)this.count));
            }
        }

        public override string ToString()
        {
            return Convert.ToString(this.average);
        }
    }
}