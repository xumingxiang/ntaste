namespace org.apache.mahout.cf.taste.impl.common
{
    using System;

    public sealed class InvertedRunningAverage : RunningAverage
    {
        private RunningAverage _Delegate;

        public RunningAverage Delegate
        {
            get { return this._Delegate; }
        }

        public InvertedRunningAverage(RunningAverage deleg)
        {
            this._Delegate = deleg;
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
            return -this._Delegate.getAverage();
        }

        public int getCount()
        {
            return this._Delegate.getCount();
        }

        public RunningAverage inverse()
        {
            return this._Delegate;
        }

        public void removeDatum(double datum)
        {
            throw new NotSupportedException();
        }
    }
}