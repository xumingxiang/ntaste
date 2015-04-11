namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.commons.math3.util;
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.eval;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.impl.model;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using org.apache.mahout.common;
    using System.Collections.Generic;
    using System.Diagnostics;

    public sealed class GenericRecommenderIRStatsEvaluator : RecommenderIRStatsEvaluator
    {
        public const double CHOOSE_THRESHOLD = double.NaN;
        private RelevantItemsDataSplitter dataSplitter;
        private static Logger log = LoggerFactory.getLogger(typeof(GenericRecommenderIRStatsEvaluator));
        private static double LOG2 = MathUtil.Log(2.0);
        private RandomWrapper random;

        public GenericRecommenderIRStatsEvaluator()
            : this(new GenericRelevantItemsDataSplitter())
        {
        }

        public GenericRecommenderIRStatsEvaluator(RelevantItemsDataSplitter dataSplitter)
        {
            this.random = RandomUtils.getRandom();
            this.dataSplitter = dataSplitter;
        }

        private static double computeThreshold(PreferenceArray prefs)
        {
            if (prefs.length() < 2)
            {
                return double.NegativeInfinity;
            }
            RunningAverageAndStdDev dev = new FullRunningAverageAndStdDev();
            int num = prefs.length();
            for (int i = 0; i < num; i++)
            {
                dev.addDatum((double)prefs.getValue(i));
            }
            return (dev.getAverage() + dev.getStandardDeviation());
        }

        public IRStatistics evaluate(RecommenderBuilder recommenderBuilder, DataModelBuilder dataModelBuilder, DataModel dataModel, IDRescorer rescorer, int at, double relevanceThreshold, double evaluationPercentage)
        {
            int num = dataModel.getNumItems();
            RunningAverage average = new FullRunningAverage();
            RunningAverage average2 = new FullRunningAverage();
            RunningAverage average3 = new FullRunningAverage();
            RunningAverage average4 = new FullRunningAverage();
            int num2 = 0;
            int num3 = 0;
            IEnumerator<long> enumerator = dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                if (this.random.nextDouble() < evaluationPercentage)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    PreferenceArray prefs = dataModel.getPreferencesFromUser(current);
                    double num5 = double.IsNaN(relevanceThreshold) ? computeThreshold(prefs) : relevanceThreshold;
                    FastIDSet relevantItemIDs = this.dataSplitter.getRelevantItemsIDs(current, at, num5, dataModel);
                    int num6 = relevantItemIDs.size();
                    if (num6 > 0)
                    {
                        FastByIDMap<PreferenceArray> trainingUsers = new FastByIDMap<PreferenceArray>(dataModel.getNumUsers());
                        IEnumerator<long> enumerator2 = dataModel.getUserIDs();
                        while (enumerator2.MoveNext())
                        {
                            this.dataSplitter.processOtherUser(current, relevantItemIDs, trainingUsers, enumerator2.Current, dataModel);
                        }
                        DataModel model = (dataModelBuilder == null) ? new GenericDataModel(trainingUsers) : dataModelBuilder.buildDataModel(trainingUsers);
                        try
                        {
                            model.getPreferencesFromUser(current);
                        }
                        catch (NoSuchUserException)
                        {
                            continue;
                        }
                        int num7 = num6 + model.getItemIDsFromUser(current).size();
                        if (num7 >= (2 * at))
                        {
                            Recommender recommender = recommenderBuilder.buildRecommender(model);
                            int num8 = 0;
                            List<RecommendedItem> list = recommender.recommend(current, at, rescorer);
                            foreach (RecommendedItem item in list)
                            {
                                if (relevantItemIDs.contains(item.getItemID()))
                                {
                                    num8++;
                                }
                            }
                            int count = list.Count;
                            if (count > 0)
                            {
                                average.addDatum(((double)num8) / ((double)count));
                            }
                            average2.addDatum(((double)num8) / ((double)num6));
                            if (num6 < num7)
                            {
                                average3.addDatum(((double)(count - num8)) / ((double)(num - num6)));
                            }
                            double num10 = 0.0;
                            double num11 = 0.0;
                            for (int i = 0; i < count; i++)
                            {
                                RecommendedItem item2 = list[i];
                                double num13 = 1.0 / log2(i + 2.0);
                                if (relevantItemIDs.contains(item2.getItemID()))
                                {
                                    num10 += num13;
                                }
                                if (i < num6)
                                {
                                    num11 += num13;
                                }
                            }
                            if (num11 > 0.0)
                            {
                                average4.addDatum(num10 / num11);
                            }
                            num2++;
                            if (count > 0)
                            {
                                num3++;
                            }
                            stopwatch.Stop();
                            log.info("Evaluated with user {} in {}ms", new object[] { current, stopwatch.ElapsedMilliseconds });
                            log.info("Precision/recall/fall-out/nDCG/reach: {} / {} / {} / {} / {}", new object[] { average.getAverage(), average2.getAverage(), average3.getAverage(), average4.getAverage(), ((double)num3) / ((double)num2) });
                        }
                    }
                }
            }
            return new IRStatisticsImpl(average.getAverage(), average2.getAverage(), average3.getAverage(), average4.getAverage(), ((double)num3) / ((double)num2));
        }

        private static double log2(double value)
        {
            return (MathUtil.Log(value) / LOG2);
        }
    }
}