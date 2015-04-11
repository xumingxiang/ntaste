namespace org.apache.mahout.cf.taste.impl.common
{
    using System;

    public sealed class InvertedRunningAverageAndStdDev : RunningAverageAndStdDev, RunningAverage
    {
        private RunningAverageAndStdDev _Delegate;

        public RunningAverageAndStdDev Delegate
        {
            get { return this._Delegate; }
        }

        public InvertedRunningAverageAndStdDev(RunningAverageAndStdDev deleg)
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

        public double getStandardDeviation()
        {
            return this._Delegate.getStandardDeviation();
        }

        public RunningAverageAndStdDev inverse()
        {
            return this._Delegate;
        }

        RunningAverage RunningAverage.inverse()
        {
            return this.inverse();
        }

        public void removeDatum(double datum)
        {
            throw new NotSupportedException();
        }
    }
}