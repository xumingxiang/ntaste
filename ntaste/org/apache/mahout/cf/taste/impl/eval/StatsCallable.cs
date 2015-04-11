namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.impl.common;
    using System;
    using System.Diagnostics;

    internal sealed class StatsCallable
    {
        private Action _Delegate;
        private static Logger log = LoggerFactory.getLogger(typeof(StatsCallable));
        private bool logStats;
        private AtomicInteger noEstimateCounter;
        private RunningAverageAndStdDev timing;

        public StatsCallable(Action deleg, bool logStats, RunningAverageAndStdDev timing, AtomicInteger noEstimateCounter)
        {
            this._Delegate = deleg;
            this.logStats = logStats;
            this.timing = timing;
            this.noEstimateCounter = noEstimateCounter;
        }

        public void call()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            this._Delegate();
            stopwatch.Stop();
            this.timing.addDatum((double)stopwatch.ElapsedMilliseconds);
            if (this.logStats)
            {
                int num = (int)this.timing.getAverage();
                log.info("Average time per recommendation: {}ms", new object[] { num });
                long totalMemory = GC.GetTotalMemory(false);
                log.info("Approximate memory used: {}MB", new object[] { totalMemory / 0xf4240L });
                log.info("Unable to recommend in {} cases", new object[] { this.noEstimateCounter.get() });
            }
        }
    }
}