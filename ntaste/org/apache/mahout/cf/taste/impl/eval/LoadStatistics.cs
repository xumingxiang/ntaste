namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste.impl.common;

    public sealed class LoadStatistics
    {
        private RunningAverage timing;

        public LoadStatistics(RunningAverage timing)
        {
            this.timing = timing;
        }

        public RunningAverage getTiming()
        {
            return this.timing;
        }
    }
}