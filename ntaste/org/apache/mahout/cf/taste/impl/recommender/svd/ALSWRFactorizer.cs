namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.common;
    using org.apache.mahout.math.als;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ALSWRFactorizer : AbstractFactorizer
    {
        private double alpha;
        private DataModel dataModel;
        private static double DEFAULT_ALPHA = 40.0;
        private double lambda;
        private static Logger log = LoggerFactory.getLogger(typeof(ALSWRFactorizer));
        private int numFeatures;
        private int numIterations;
        private int numTrainingThreads;
        private bool usesImplicitFeedback;

        public ALSWRFactorizer(DataModel dataModel, int numFeatures, double lambda, int numIterations)
            : this(dataModel, numFeatures, lambda, numIterations, false, DEFAULT_ALPHA)
        {
        }

        public ALSWRFactorizer(DataModel dataModel, int numFeatures, double lambda, int numIterations, bool usesImplicitFeedback, double alpha)
            : this(dataModel, numFeatures, lambda, numIterations, usesImplicitFeedback, alpha, Environment.ProcessorCount)
        {
        }

        public ALSWRFactorizer(DataModel dataModel, int numFeatures, double lambda, int numIterations, bool usesImplicitFeedback, double alpha, int numTrainingThreads)
            : base(dataModel)
        {
            this.dataModel = dataModel;
            this.numFeatures = numFeatures;
            this.lambda = lambda;
            this.numIterations = numIterations;
            this.usesImplicitFeedback = usesImplicitFeedback;
            this.alpha = alpha;
            this.numTrainingThreads = numTrainingThreads;
        }

        public override Factorization factorize()
        {
            log.info("starting to compute the factorization...", new object[0]);
            Features features = new Features(this);
            IDictionary<int, double[]> y = null;
            IDictionary<int, double[]> dictionary2 = null;
            if (this.usesImplicitFeedback)
            {
                y = this.userFeaturesMapping(this.dataModel.getUserIDs(), this.dataModel.getNumUsers(), features.getU());
                dictionary2 = this.itemFeaturesMapping(this.dataModel.getItemIDs(), this.dataModel.getNumItems(), features.getM());
            }
            for (int i = 0; i < this.numIterations; i++)
            {
                AggregateException exception;
                log.info("iteration {0}", new object[] { i });
                IList<Task> source = new List<Task>();
                IEnumerator<long> enumerator = this.dataModel.getUserIDs();
                try
                {
                    ImplicitFeedbackAlternatingLeastSquaresSolver implicitFeedbackSolver = this.usesImplicitFeedback ? new ImplicitFeedbackAlternatingLeastSquaresSolver(this.numFeatures, this.lambda, this.alpha, dictionary2) : null;
                    while (enumerator.MoveNext())
                    {
                        long userID = enumerator.Current;
                        IEnumerator<long> itemIDsFromUser = this.dataModel.getItemIDsFromUser(userID).GetEnumerator();
                        PreferenceArray userPrefs = this.dataModel.getPreferencesFromUser(userID);
                        source.Add(Task.Factory.StartNew(delegate
                        {
                            List<double[]> featureVectors = new List<double[]>();
                            while (itemIDsFromUser.MoveNext())
                            {
                                long current = itemIDsFromUser.Current;
                                featureVectors.Add(features.getItemFeatureColumn(this.itemIndex(current)));
                            }
                            double[] vector = this.usesImplicitFeedback ? implicitFeedbackSolver.solve(this.sparseUserRatingVector(userPrefs)) : AlternatingLeastSquaresSolver.solve(featureVectors, ratingVector(userPrefs), this.lambda, this.numFeatures);
                            features.setFeatureColumnInU(this.userIndex(userID), vector);
                        }));
                    }
                }
                finally
                {
                    try
                    {
                        Task.WaitAll(source.ToArray<Task>(), (int)(0x3e8 * this.dataModel.getNumUsers()));
                    }
                    catch (AggregateException exception1)
                    {
                        exception = exception1;
                        log.warn("Error when computing user features", new object[] { exception });
                        throw exception;
                    }
                }
                source = new List<Task>();
                IEnumerator<long> enumerator2 = this.dataModel.getItemIDs();
                try
                {
                    ImplicitFeedbackAlternatingLeastSquaresSolver implicitFeedbackSolver = this.usesImplicitFeedback ? new ImplicitFeedbackAlternatingLeastSquaresSolver(this.numFeatures, this.lambda, this.alpha, y) : null;
                    while (enumerator2.MoveNext())
                    {
                        long itemID = enumerator2.Current;
                        PreferenceArray itemPrefs = this.dataModel.getPreferencesForItem(itemID);
                        source.Add(Task.Factory.StartNew(delegate
                        {
                            List<double[]> featureVectors = new List<double[]>();
                            foreach (Preference preference in itemPrefs)
                            {
                                long num1 = preference.getUserID();
                                featureVectors.Add(features.getUserFeatureColumn(this.userIndex(num1)));
                            }
                            double[] vector = this.usesImplicitFeedback ? implicitFeedbackSolver.solve(this.sparseItemRatingVector(itemPrefs)) : AlternatingLeastSquaresSolver.solve(featureVectors, ratingVector(itemPrefs), this.lambda, this.numFeatures);
                            features.setFeatureColumnInM(this.itemIndex(itemID), vector);
                        }));
                    }
                }
                finally
                {
                    try
                    {
                        Task.WaitAll(source.ToArray<Task>(), (int)(0x3e8 * this.dataModel.getNumItems()));
                    }
                    catch (AggregateException exception2)
                    {
                        exception = exception2;
                        log.warn("Error when computing item features", new object[] { exception });
                        throw exception;
                    }
                }
            }
            log.info("finished computation of the factorization...", new object[0]);
            return base.createFactorization(features.getU(), features.getM());
        }

        protected IDictionary<int, double[]> itemFeaturesMapping(IEnumerator<long> itemIDs, int numItems, double[][] featureMatrix)
        {
            Dictionary<int, double[]> dictionary = new Dictionary<int, double[]>(numItems);
            while (itemIDs.MoveNext())
            {
                long current = itemIDs.Current;
                dictionary[(int)current] = featureMatrix[base.itemIndex(current)];
            }
            return dictionary;
        }

        public static double[] ratingVector(PreferenceArray prefs)
        {
            double[] numArray = new double[prefs.length()];
            for (int i = 0; i < prefs.length(); i++)
            {
                numArray[i] = prefs.get(i).getValue();
            }
            return numArray;
        }

        protected IList<Tuple<int, double>> sparseItemRatingVector(PreferenceArray prefs)
        {
            List<Tuple<int, double>> list = new List<Tuple<int, double>>(prefs.length());
            foreach (Preference preference in prefs)
            {
                list.Add(new Tuple<int, double>((int)preference.getUserID(), (double)preference.getValue()));
            }
            return list;
        }

        protected IList<Tuple<int, double>> sparseUserRatingVector(PreferenceArray prefs)
        {
            List<Tuple<int, double>> list = new List<Tuple<int, double>>(prefs.length());
            foreach (Preference preference in prefs)
            {
                list.Add(new Tuple<int, double>((int)preference.getItemID(), (double)preference.getValue()));
            }
            return list;
        }

        protected IDictionary<int, double[]> userFeaturesMapping(IEnumerator<long> userIDs, int numUsers, double[][] featureMatrix)
        {
            Dictionary<int, double[]> dictionary = new Dictionary<int, double[]>(numUsers);
            while (userIDs.MoveNext())
            {
                long current = userIDs.Current;
                dictionary[(int)current] = featureMatrix[base.userIndex(current)];
            }
            return dictionary;
        }

        public class Features
        {
            private DataModel dataModel;
            private double[][] M;
            private int numFeatures;
            private double[][] U;

            public Features(ALSWRFactorizer factorizer)
            {
                this.dataModel = factorizer.dataModel;
                this.numFeatures = factorizer.numFeatures;
                RandomWrapper wrapper = RandomUtils.getRandom();
                this.M = new double[this.dataModel.getNumItems()][];
                IEnumerator<long> enumerator = this.dataModel.getItemIDs();
                while (enumerator.MoveNext())
                {
                    long current = enumerator.Current;
                    int index = factorizer.itemIndex(current);
                    this.M[index] = new double[this.numFeatures];
                    this.M[index][0] = this.averateRating(current);
                    for (int j = 1; j < this.numFeatures; j++)
                    {
                        this.M[index][j] = wrapper.nextDouble() * 0.1;
                    }
                }
                this.U = new double[this.dataModel.getNumUsers()][];
                for (int i = 0; i < this.U.Length; i++)
                {
                    this.U[i] = new double[this.numFeatures];
                }
            }

            public double averateRating(long itemID)
            {
                PreferenceArray array = this.dataModel.getPreferencesForItem(itemID);
                RunningAverage average = new FullRunningAverage();
                foreach (Preference preference in array)
                {
                    average.addDatum((double)preference.getValue());
                }
                return average.getAverage();
            }

            public double[] getItemFeatureColumn(int index)
            {
                return this.M[index];
            }

            public double[][] getM()
            {
                return this.M;
            }

            public double[][] getU()
            {
                return this.U;
            }

            public double[] getUserFeatureColumn(int index)
            {
                return this.U[index];
            }

            protected void setFeatureColumn(double[][] matrix, int idIndex, double[] vector)
            {
                for (int i = 0; i < this.numFeatures; i++)
                {
                    matrix[idIndex][i] = vector[i];
                }
            }

            public void setFeatureColumnInM(int idIndex, double[] vector)
            {
                this.setFeatureColumn(this.M, idIndex, vector);
            }

            public void setFeatureColumnInU(int idIndex, double[] vector)
            {
                this.setFeatureColumn(this.U, idIndex, vector);
            }
        }
    }
}