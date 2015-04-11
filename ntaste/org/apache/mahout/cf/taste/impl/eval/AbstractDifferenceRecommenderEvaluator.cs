namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.eval;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.impl.model;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using org.apache.mahout.common;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public abstract class AbstractDifferenceRecommenderEvaluator : RecommenderEvaluator
    {
        private static Logger log = LoggerFactory.getLogger(typeof(AbstractDifferenceRecommenderEvaluator));
        private float maxPreference = float.NaN;
        private float minPreference = float.NaN;
        private RandomWrapper random = RandomUtils.getRandom();

        protected AbstractDifferenceRecommenderEvaluator()
        {
        }

        private float capEstimatedPreference(float estimate)
        {
            if (estimate > this.maxPreference)
            {
                return this.maxPreference;
            }
            if (estimate < this.minPreference)
            {
                return this.minPreference;
            }
            return estimate;
        }

        protected abstract double computeFinalEvaluation();

        public virtual double evaluate(RecommenderBuilder recommenderBuilder, DataModelBuilder dataModelBuilder, DataModel dataModel, double trainingPercentage, double evaluationPercentage)
        {
            log.info("Beginning evaluation using {} of {}", new object[] { trainingPercentage, dataModel });
            int num = dataModel.getNumUsers();
            FastByIDMap<PreferenceArray> trainingPrefs = new FastByIDMap<PreferenceArray>(1 + ((int)(evaluationPercentage * num)));
            FastByIDMap<PreferenceArray> testPrefs = new FastByIDMap<PreferenceArray>(1 + ((int)(evaluationPercentage * num)));
            IEnumerator<long> enumerator = dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                if (this.random.nextDouble() < evaluationPercentage)
                {
                    this.splitOneUsersPrefs(trainingPercentage, trainingPrefs, testPrefs, current, dataModel);
                }
            }
            DataModel model = (dataModelBuilder == null) ? new GenericDataModel(trainingPrefs) : dataModelBuilder.buildDataModel(trainingPrefs);
            Recommender recommender = recommenderBuilder.buildRecommender(model);
            double num3 = this.getEvaluation(testPrefs, recommender);
            log.info("Evaluation result: {}", new object[] { num3 });
            return num3;
        }

        public static void execute(List<Action> callables, AtomicInteger noEstimateCounter, RunningAverageAndStdDev timing)
        {
            List<Action> list = wrapWithStatsCallables(callables, noEstimateCounter, timing);
            int processorCount = Environment.ProcessorCount;
            Task[] tasks = new Task[list.Count];
            log.info("Starting timing of {} tasks in {} threads", new object[] { list.Count, processorCount });
            try
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Factory.StartNew(list[i]);
                }
                Task.WaitAll(tasks, 0x2710);
            }
            catch (Exception exception)
            {
                throw new TasteException(exception.Message, exception);
            }
        }

        private double getEvaluation(FastByIDMap<PreferenceArray> testPrefs, Recommender recommender)
        {
            this.reset();
            List<Action> callables = new List<Action>();
            AtomicInteger noEstimateCounter = new AtomicInteger();
            using (IEnumerator<KeyValuePair<long, PreferenceArray>> enumerator = testPrefs.entrySet().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Action item = null;
                    KeyValuePair<long, PreferenceArray> entry = enumerator.Current;
                    if (item == null)
                    {
                        item = delegate
                        {
                            long userID = entry.Key;
                            PreferenceArray array = entry.Value;
                            foreach (Preference preference in array)
                            {
                                float f = float.NaN;
                                try
                                {
                                    f = recommender.estimatePreference(userID, preference.getItemID());
                                }
                                catch (NoSuchUserException)
                                {
                                    log.info("User exists in test data but not training data: {}", new object[] { userID });
                                }
                                catch (NoSuchItemException)
                                {
                                    log.info("Item exists in test data but not training data: {}", new object[] { preference.getItemID() });
                                }
                                if (float.IsNaN(f))
                                {
                                    noEstimateCounter.incrementAndGet();
                                }
                                else
                                {
                                    f = this.capEstimatedPreference(f);
                                    this.processOneEstimate(f, preference);
                                }
                            }
                        };
                    }
                    callables.Add(item);
                }
            }
            log.info("Beginning evaluation of {} users", new object[] { callables.Count });
            RunningAverageAndStdDev timing = new FullRunningAverageAndStdDev();
            execute(callables, noEstimateCounter, timing);
            return this.computeFinalEvaluation();
        }

        public virtual float getMaxPreference()
        {
            return this.maxPreference;
        }

        public virtual float getMinPreference()
        {
            return this.minPreference;
        }

        protected abstract void processOneEstimate(float estimatedPreference, Preference realPref);

        protected abstract void reset();

        public virtual void setMaxPreference(float maxPreference)
        {
            this.maxPreference = maxPreference;
        }

        public virtual void setMinPreference(float minPreference)
        {
            this.minPreference = minPreference;
        }

        private void splitOneUsersPrefs(double trainingPercentage, FastByIDMap<PreferenceArray> trainingPrefs, FastByIDMap<PreferenceArray> testPrefs, long userID, DataModel dataModel)
        {
            List<Preference> prefs = null;
            List<Preference> list2 = null;
            PreferenceArray array = dataModel.getPreferencesFromUser(userID);
            int num = array.length();
            for (int i = 0; i < num; i++)
            {
                Preference item = new GenericPreference(userID, array.getItemID(i), array.getValue(i));
                if (this.random.nextDouble() < trainingPercentage)
                {
                    if (prefs == null)
                    {
                        prefs = new List<Preference>(3);
                    }
                    prefs.Add(item);
                }
                else
                {
                    if (list2 == null)
                    {
                        list2 = new List<Preference>(3);
                    }
                    list2.Add(item);
                }
            }
            if (prefs != null)
            {
                trainingPrefs.put(userID, new GenericUserPreferenceArray(prefs));
                if (list2 != null)
                {
                    testPrefs.put(userID, new GenericUserPreferenceArray(list2));
                }
            }
        }

        private static List<Action> wrapWithStatsCallables(List<Action> callables, AtomicInteger noEstimateCounter, RunningAverageAndStdDev timing)
        {
            List<Action> list = new List<Action>();
            for (int i = 0; i < callables.Count; i++)
            {
                bool logStats = (i % 0x3e8) == 0;
                list.Add(new Action(new StatsCallable(callables[i], logStats, timing, noEstimateCounter).call));
            }
            return list;
        }
    }
}