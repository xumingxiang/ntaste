namespace org.apache.mahout.cf.taste.impl.common
{
    using System;

    public class WeightedRunningAverage : RunningAverage
    {
        private double average = double.NaN;
        private double totalWeight = 0.0;

        public virtual void addDatum(double datum)
        {
            this.addDatum(datum, 1.0);
        }

        public virtual void addDatum(double datum, double weight)
        {
            double totalWeight = this.totalWeight;
            this.totalWeight += weight;
            if (totalWeight <= 0.0)
            {
                this.average = datum;
            }
            else
            {
                this.average = ((this.average * totalWeight) / this.totalWeight) + ((datum * weight) / this.totalWeight);
            }
        }

        public virtual void changeDatum(double delta)
        {
            this.changeDatum(delta, 1.0);
        }

        public virtual void changeDatum(double delta, double weight)
        {
            this.average += (delta * weight) / this.totalWeight;
        }

        public virtual double getAverage()
        {
            return this.average;
        }

        public virtual int getCount()
        {
            return (int)this.totalWeight;
        }

        public virtual double getTotalWeight()
        {
            return this.totalWeight;
        }

        public virtual RunningAverage inverse()
        {
            return new InvertedRunningAverage(this);
        }

        public virtual void removeDatum(double datum)
        {
            this.removeDatum(datum, 1.0);
        }

        public virtual void removeDatum(double datum, double weight)
        {
            double totalWeight = this.totalWeight;
            this.totalWeight -= weight;
            if (this.totalWeight <= 0.0)
            {
                this.average = double.NaN;
                this.totalWeight = 0.0;
            }
            else
            {
                this.average = ((this.average * totalWeight) / this.totalWeight) - ((datum * weight) / this.totalWeight);
            }
        }

        public override string ToString()
        {
            return Convert.ToString(this.average);
        }
    }
}