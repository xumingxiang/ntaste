namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System;
    using System.Collections.Generic;

    public sealed class LoadEvaluator
    {
        private LoadEvaluator()
        {
        }

        public static LoadStatistics runLoad(Recommender recommender)
        {
            return runLoad(recommender, 10);
        }

        public static LoadStatistics runLoad(Recommender recommender, int howMany)
        {
            DataModel model = recommender.getDataModel();
            int num = model.getNumUsers();
            double samplingRate = 1000.0 / ((double)num);
            IEnumerator<long> enumerator = SamplingLongPrimitiveIterator.maybeWrapIterator(model.getUserIDs(), samplingRate);
            if (enumerator.MoveNext())
            {
                recommender.recommend(enumerator.Current, howMany);
            }
            List<Action> callables = new List<Action>();
            while (enumerator.MoveNext())
            {
                callables.Add(new Action(new LoadCallable(recommender, enumerator.Current).call));
            }
            AtomicInteger noEstimateCounter = new AtomicInteger();
            RunningAverageAndStdDev timing = new FullRunningAverageAndStdDev();
            AbstractDifferenceRecommenderEvaluator.execute(callables, noEstimateCounter, timing);
            return new LoadStatistics(timing);
        }
    }
}