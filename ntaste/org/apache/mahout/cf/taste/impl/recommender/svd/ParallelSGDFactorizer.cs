namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.common;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ParallelSGDFactorizer : AbstractFactorizer
    {
        private double biasLambdaRatio;
        private double biasMuRatio;
        private DataModel dataModel;
        private double decayFactor;
        private int epoch;
        private static int FEATURE_OFFSET = 3;
        private double forgettingExponent;
        private static int ITEM_BIAS_INDEX = 2;
        protected volatile double[][] itemVectors;
        private double lambda;
        private static Logger logger = LoggerFactory.getLogger(typeof(ParallelSGDFactorizer));
        private double mu0;
        private static double NOISE = 0.02;
        private int numEpochs;
        private int numThreads;
        private int rank;
        private PreferenceShuffler shuffler;
        private int stepOffset;
        private static int USER_BIAS_INDEX = 1;
        protected volatile double[][] userVectors;

        public ParallelSGDFactorizer(DataModel dataModel, int numFeatures, double lambda, int numEpochs)
            : base(dataModel)
        {
            this.mu0 = 0.01;
            this.decayFactor = 1.0;
            this.stepOffset = 0;
            this.forgettingExponent = 0.0;
            this.biasMuRatio = 0.5;
            this.biasLambdaRatio = 0.1;
            this.epoch = 1;
            this.dataModel = dataModel;
            this.rank = numFeatures + FEATURE_OFFSET;
            this.lambda = lambda;
            this.numEpochs = numEpochs;
            this.shuffler = new PreferenceShuffler(dataModel);
            this.numThreads = Math.Min(Environment.ProcessorCount, (int)Math.Pow((double)this.shuffler.size(), 0.25));
        }

        public ParallelSGDFactorizer(DataModel dataModel, int numFeatures, double lambda, int numIterations, double mu0, double decayFactor, int stepOffset, double forgettingExponent)
            : this(dataModel, numFeatures, lambda, numIterations)
        {
            this.mu0 = mu0;
            this.decayFactor = decayFactor;
            this.stepOffset = stepOffset;
            this.forgettingExponent = forgettingExponent;
        }

        public ParallelSGDFactorizer(DataModel dataModel, int numFeatures, double lambda, int numIterations, double mu0, double decayFactor, int stepOffset, double forgettingExponent, int numThreads)
            : this(dataModel, numFeatures, lambda, numIterations, mu0, decayFactor, stepOffset, forgettingExponent)
        {
            this.numThreads = numThreads;
        }

        public ParallelSGDFactorizer(DataModel dataModel, int numFeatures, double lambda, int numIterations, double mu0, double decayFactor, int stepOffset, double forgettingExponent, double biasMuRatio, double biasLambdaRatio)
            : this(dataModel, numFeatures, lambda, numIterations, mu0, decayFactor, stepOffset, forgettingExponent)
        {
            this.biasMuRatio = biasMuRatio;
            this.biasLambdaRatio = biasLambdaRatio;
        }

        public ParallelSGDFactorizer(DataModel dataModel, int numFeatures, double lambda, int numIterations, double mu0, double decayFactor, int stepOffset, double forgettingExponent, double biasMuRatio, double biasLambdaRatio, int numThreads)
            : this(dataModel, numFeatures, lambda, numIterations, mu0, decayFactor, stepOffset, forgettingExponent, biasMuRatio, biasLambdaRatio)
        {
            this.numThreads = numThreads;
        }

        private double dot(double[] userVector, double[] itemVector)
        {
            double num = 0.0;
            for (int i = 0; i < this.rank; i++)
            {
                num += userVector[i] * itemVector[i];
            }
            return num;
        }

        public override Factorization factorize()
        {
            this.initialize();
            logger.info("starting to compute the factorization...", new object[0]);
            this.epoch = 1;
            while (this.epoch <= this.numEpochs)
            {
                this.shuffler.stage();
                double mu = this.getMu(this.epoch);
                int num = (this.shuffler.size() / this.numThreads) + 1;
                Task[] tasks = new Task[this.numThreads];
                try
                {
                    for (int j = 0; j < this.numThreads; j++)
                    {
                        int iStart = j * num;
                        int iEnd = Math.Min((j + 1) * num, this.shuffler.size());
                        tasks[j] = Task.Factory.StartNew(delegate
                        {
                            for (int k = iStart; k < iEnd; k++)
                            {
                                this.update(this.shuffler.get(k), mu);
                            }
                        });
                    }
                }
                finally
                {
                    Task.WaitAll(tasks, (int)(this.numEpochs * this.shuffler.size()));
                    this.shuffler.shuffle();
                }
                this.epoch++;
            }
            return base.createFactorization(this.userVectors, this.itemVectors);
        }

        private double getAveragePreference()
        {
            RunningAverage average = new FullRunningAverage();
            IEnumerator<long> enumerator = this.dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                foreach (Preference preference in this.dataModel.getPreferencesFromUser(enumerator.Current))
                {
                    average.addDatum((double)preference.getValue());
                }
            }
            return average.getAverage();
        }

        private double getMu(int i)
        {
            return ((this.mu0 * Math.Pow(this.decayFactor, (double)(i - 1))) * Math.Pow((double)(i + this.stepOffset), this.forgettingExponent));
        }

        protected void initialize()
        {
            int num3;
            RandomWrapper wrapper = RandomUtils.getRandom();
            this.userVectors = new double[this.dataModel.getNumUsers()][];
            this.itemVectors = new double[this.dataModel.getNumItems()][];
            double num = this.getAveragePreference();
            for (int i = 0; i < this.userVectors.Length; i++)
            {
                this.userVectors[i] = new double[this.rank];
                this.userVectors[i][0] = num;
                this.userVectors[i][USER_BIAS_INDEX] = 0.0;
                this.userVectors[i][ITEM_BIAS_INDEX] = 1.0;
                num3 = FEATURE_OFFSET;
                while (num3 < this.rank)
                {
                    this.userVectors[i][num3] = wrapper.nextGaussian() * NOISE;
                    num3++;
                }
            }
            for (int j = 0; j < this.itemVectors.Length; j++)
            {
                this.itemVectors[j] = new double[this.rank];
                this.itemVectors[j][0] = 1.0;
                this.itemVectors[j][USER_BIAS_INDEX] = 1.0;
                this.itemVectors[j][ITEM_BIAS_INDEX] = 0.0;
                for (num3 = FEATURE_OFFSET; num3 < this.rank; num3++)
                {
                    this.itemVectors[j][num3] = wrapper.nextGaussian() * NOISE;
                }
            }
        }

        protected void update(Preference preference, double mu)
        {
            int index = base.userIndex(preference.getUserID());
            int num2 = base.itemIndex(preference.getItemID());
            double[] userVector = this.userVectors[index];
            double[] itemVector = this.itemVectors[num2];
            double num3 = this.dot(userVector, itemVector);
            double num4 = preference.getValue() - num3;
            for (int i = FEATURE_OFFSET; i < this.rank; i++)
            {
                double num6 = userVector[i];
                double num7 = itemVector[i];
                userVector[i] += mu * ((num4 * num7) - (this.lambda * num6));
                itemVector[i] += mu * ((num4 * num6) - (this.lambda * num7));
            }
            userVector[USER_BIAS_INDEX] += (this.biasMuRatio * mu) * (num4 - ((this.biasLambdaRatio * this.lambda) * userVector[USER_BIAS_INDEX]));
            itemVector[ITEM_BIAS_INDEX] += (this.biasMuRatio * mu) * (num4 - ((this.biasLambdaRatio * this.lambda) * itemVector[ITEM_BIAS_INDEX]));
        }

        public class PreferenceShuffler
        {
            private Preference[] preferences;
            protected RandomWrapper random = RandomUtils.getRandom();
            private Preference[] unstagedPreferences;

            public PreferenceShuffler(DataModel dataModel)
            {
                this.cachePreferences(dataModel);
                this.shuffle();
                this.stage();
            }

            private void cachePreferences(DataModel dataModel)
            {
                int num = this.countPreferences(dataModel);
                this.preferences = new Preference[num];
                IEnumerator<long> enumerator = dataModel.getUserIDs();
                int num2 = 0;
                while (enumerator.MoveNext())
                {
                    long current = enumerator.Current;
                    PreferenceArray array = dataModel.getPreferencesFromUser(current);
                    foreach (Preference preference in array)
                    {
                        this.preferences[num2++] = preference;
                    }
                }
            }

            private int countPreferences(DataModel dataModel)
            {
                int num = 0;
                IEnumerator<long> enumerator = dataModel.getUserIDs();
                while (enumerator.MoveNext())
                {
                    PreferenceArray array = dataModel.getPreferencesFromUser(enumerator.Current);
                    num += array.length();
                }
                return num;
            }

            public Preference get(int i)
            {
                return this.preferences[i];
            }

            public void shuffle()
            {
                this.unstagedPreferences = (Preference[])this.preferences.Clone();
                for (int i = this.unstagedPreferences.Length - 1; i > 0; i--)
                {
                    int y = this.random.nextInt(i + 1);
                    this.swapCachedPreferences(i, y);
                }
            }

            public int size()
            {
                return this.preferences.Length;
            }

            public void stage()
            {
                this.preferences = this.unstagedPreferences;
            }

            private void swapCachedPreferences(int x, int y)
            {
                Preference preference = this.unstagedPreferences[x];
                this.unstagedPreferences[x] = this.unstagedPreferences[y];
                this.unstagedPreferences[y] = preference;
            }
        }
    }
}