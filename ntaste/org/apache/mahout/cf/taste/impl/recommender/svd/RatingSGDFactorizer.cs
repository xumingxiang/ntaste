namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.common;
    using System.Collections.Generic;

    public class RatingSGDFactorizer : AbstractFactorizer
    {
        protected double biasLearningRate;
        protected double biasReg;
        private long[] cachedItemIDs;
        private long[] cachedUserIDs;
        protected DataModel dataModel;
        protected static readonly int FEATURE_OFFSET = 3;
        protected static int ITEM_BIAS_INDEX = 2;
        protected double[][] itemVectors;
        protected double learningRate;
        protected double learningRateDecay;
        protected int numFeatures;
        private int numIterations;
        protected double preventOverfitting;
        protected double randomNoise;
        protected static int USER_BIAS_INDEX = 1;
        protected double[][] userVectors;

        public RatingSGDFactorizer(DataModel dataModel, int numFeatures, int numIterations)
            : this(dataModel, numFeatures, 0.01, 0.1, 0.01, numIterations, 1.0)
        {
        }

        public RatingSGDFactorizer(DataModel dataModel, int numFeatures, double learningRate, double preventOverfitting, double randomNoise, int numIterations, double learningRateDecay)
            : base(dataModel)
        {
            this.biasLearningRate = 0.5;
            this.biasReg = 0.1;
            this.dataModel = dataModel;
            this.numFeatures = numFeatures + FEATURE_OFFSET;
            this.numIterations = numIterations;
            this.learningRate = learningRate;
            this.learningRateDecay = learningRateDecay;
            this.preventOverfitting = preventOverfitting;
            this.randomNoise = randomNoise;
        }

        private void cachePreferences()
        {
            int num = this.countPreferences();
            this.cachedUserIDs = new long[num];
            this.cachedItemIDs = new long[num];
            IEnumerator<long> enumerator = this.dataModel.getUserIDs();
            int index = 0;
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                PreferenceArray array = this.dataModel.getPreferencesFromUser(current);
                foreach (Preference preference in array)
                {
                    this.cachedUserIDs[index] = current;
                    this.cachedItemIDs[index] = preference.getItemID();
                    index++;
                }
            }
        }

        private int countPreferences()
        {
            int num = 0;
            IEnumerator<long> enumerator = this.dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                PreferenceArray array = this.dataModel.getPreferencesFromUser(enumerator.Current);
                num += array.length();
            }
            return num;
        }

        public override Factorization factorize()
        {
            this.prepareTraining();
            double learningRate = this.learningRate;
            for (int i = 0; i < this.numIterations; i++)
            {
                for (int j = 0; j < this.cachedUserIDs.Length; j++)
                {
                    long userID = this.cachedUserIDs[j];
                    long itemID = this.cachedItemIDs[j];
                    float? nullable = this.dataModel.getPreferenceValue(userID, itemID);
                    this.updateParameters(userID, itemID, nullable.Value, learningRate);
                }
                learningRate *= this.learningRateDecay;
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

        private double predictRating(int userID, int itemID)
        {
            double num = 0.0;
            for (int i = 0; i < this.numFeatures; i++)
            {
                num += this.userVectors[userID][i] * this.itemVectors[itemID][i];
            }
            return num;
        }

        protected virtual void prepareTraining()
        {
            int num3;
            RandomWrapper wrapper = RandomUtils.getRandom();
            this.userVectors = new double[this.dataModel.getNumUsers()][];
            this.itemVectors = new double[this.dataModel.getNumItems()][];
            double num = this.getAveragePreference();
            for (int i = 0; i < this.userVectors.Length; i++)
            {
                this.userVectors[i] = new double[this.numFeatures];
                this.userVectors[i][0] = num;
                this.userVectors[i][USER_BIAS_INDEX] = 0.0;
                this.userVectors[i][ITEM_BIAS_INDEX] = 1.0;
                num3 = FEATURE_OFFSET;
                while (num3 < this.numFeatures)
                {
                    this.userVectors[i][num3] = wrapper.nextGaussian() * this.randomNoise;
                    num3++;
                }
            }
            for (int j = 0; j < this.itemVectors.Length; j++)
            {
                this.itemVectors[j] = new double[this.numFeatures];
                this.itemVectors[j][0] = 1.0;
                this.itemVectors[j][USER_BIAS_INDEX] = 1.0;
                this.itemVectors[j][ITEM_BIAS_INDEX] = 0.0;
                for (num3 = FEATURE_OFFSET; num3 < this.numFeatures; num3++)
                {
                    this.itemVectors[j][num3] = wrapper.nextGaussian() * this.randomNoise;
                }
            }
            this.cachePreferences();
            this.shufflePreferences();
        }

        protected void shufflePreferences()
        {
            RandomWrapper wrapper = RandomUtils.getRandom();
            for (int i = this.cachedUserIDs.Length - 1; i > 0; i--)
            {
                int posB = wrapper.nextInt(i + 1);
                this.swapCachedPreferences(i, posB);
            }
        }

        private void swapCachedPreferences(int posA, int posB)
        {
            long num = this.cachedUserIDs[posA];
            long num2 = this.cachedItemIDs[posA];
            this.cachedUserIDs[posA] = this.cachedUserIDs[posB];
            this.cachedItemIDs[posA] = this.cachedItemIDs[posB];
            this.cachedUserIDs[posB] = num;
            this.cachedItemIDs[posB] = num2;
        }

        protected void updateParameters(long userID, long itemID, float rating, double currentLearningRate)
        {
            int index = base.userIndex(userID);
            int num2 = base.itemIndex(itemID);
            double[] numArray = this.userVectors[index];
            double[] numArray2 = this.itemVectors[num2];
            double num3 = this.predictRating(index, num2);
            double num4 = rating - num3;
            numArray[USER_BIAS_INDEX] += (this.biasLearningRate * currentLearningRate) * (num4 - ((this.biasReg * this.preventOverfitting) * numArray[USER_BIAS_INDEX]));
            numArray2[ITEM_BIAS_INDEX] += (this.biasLearningRate * currentLearningRate) * (num4 - ((this.biasReg * this.preventOverfitting) * numArray2[ITEM_BIAS_INDEX]));
            for (int i = FEATURE_OFFSET; i < this.numFeatures; i++)
            {
                double num6 = numArray[i];
                double num7 = numArray2[i];
                double num8 = (num4 * num7) - (this.preventOverfitting * num6);
                numArray[i] += currentLearningRate * num8;
                double num9 = (num4 * num6) - (this.preventOverfitting * num7);
                numArray2[i] += currentLearningRate * num9;
            }
        }
    }
}