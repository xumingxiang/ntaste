namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.common;
    using System;
    using System.Collections.Generic;

    public sealed class SVDPlusPlusFactorizer : RatingSGDFactorizer
    {
        private IDictionary<int, List<int>> itemsByUser;
        private double[][] p;
        private double[][] y;

        public SVDPlusPlusFactorizer(DataModel dataModel, int numFeatures, int numIterations)
            : this(dataModel, numFeatures, 0.01, 0.1, 0.01, numIterations, 1.0)
        {
            base.biasLearningRate = 0.7;
            base.biasReg = 0.33;
        }

        public SVDPlusPlusFactorizer(DataModel dataModel, int numFeatures, double learningRate, double preventOverfitting, double randomNoise, int numIterations, double learningRateDecay)
            : base(dataModel, numFeatures, learningRate, preventOverfitting, randomNoise, numIterations, learningRateDecay)
        {
        }

        public override Factorization factorize()
        {
            this.prepareTraining();
            base.factorize();
            for (int i = 0; i < base.userVectors.Length; i++)
            {
                int num3;
                foreach (int num2 in this.itemsByUser[i])
                {
                    num3 = RatingSGDFactorizer.FEATURE_OFFSET;
                    while (num3 < base.numFeatures)
                    {
                        base.userVectors[i][num3] += this.y[num2][num3];
                        num3++;
                    }
                }
                double num4 = Math.Sqrt((double)this.itemsByUser[i].Count);
                for (num3 = 0; num3 < base.userVectors[i].Length; num3++)
                {
                    base.userVectors[i][num3] = (float)((base.userVectors[i][num3] / num4) + this.p[i][num3]);
                }
            }
            return base.createFactorization(base.userVectors, base.itemVectors);
        }

        private double predictRating(double[] userVector, int itemID)
        {
            double num = 0.0;
            for (int i = 0; i < base.numFeatures; i++)
            {
                num += userVector[i] * base.itemVectors[itemID][i];
            }
            return num;
        }

        protected override void prepareTraining()
        {
            int num;
            int num2;
            base.prepareTraining();
            RandomWrapper wrapper = RandomUtils.getRandom();
            this.p = new double[base.dataModel.getNumUsers()][];
            for (num = 0; num < this.p.Length; num++)
            {
                this.p[num] = new double[base.numFeatures];
                num2 = 0;
                while (num2 < RatingSGDFactorizer.FEATURE_OFFSET)
                {
                    this.p[num][num2] = 0.0;
                    num2++;
                }
                num2 = RatingSGDFactorizer.FEATURE_OFFSET;
                while (num2 < base.numFeatures)
                {
                    this.p[num][num2] = wrapper.nextGaussian() * base.randomNoise;
                    num2++;
                }
            }
            this.y = new double[base.dataModel.getNumItems()][];
            for (num = 0; num < this.y.Length; num++)
            {
                this.y[num] = new double[base.numFeatures];
                num2 = 0;
                while (num2 < RatingSGDFactorizer.FEATURE_OFFSET)
                {
                    this.y[num][num2] = 0.0;
                    num2++;
                }
                for (num2 = RatingSGDFactorizer.FEATURE_OFFSET; num2 < base.numFeatures; num2++)
                {
                    this.y[num][num2] = wrapper.nextGaussian() * base.randomNoise;
                }
            }
            this.itemsByUser = new Dictionary<int, List<int>>();
            IEnumerator<long> enumerator = base.dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                int num4 = base.userIndex(current);
                FastIDSet set = base.dataModel.getItemIDsFromUser(current);
                List<int> list = new List<int>(set.size());
                this.itemsByUser[num4] = list;
                foreach (long num5 in set)
                {
                    int item = base.itemIndex(num5);
                    list.Add(item);
                }
            }
        }

        protected void updateParameters(long userID, long itemID, float rating, double currentLearningRate)
        {
            int num6;
            int index = base.userIndex(userID);
            int num2 = base.itemIndex(itemID);
            double[] numArray = this.p[index];
            double[] numArray2 = base.itemVectors[num2];
            double[] userVector = new double[base.numFeatures];
            foreach (int num3 in this.itemsByUser[index])
            {
                for (int i = RatingSGDFactorizer.FEATURE_OFFSET; i < base.numFeatures; i++)
                {
                    userVector[i] += this.y[num3][i];
                }
            }
            double num5 = Math.Sqrt((double)this.itemsByUser[index].Count);
            for (num6 = 0; num6 < userVector.Length; num6++)
            {
                userVector[num6] = (float)((userVector[num6] / num5) + this.p[index][num6]);
            }
            double num7 = this.predictRating(userVector, num2);
            double num8 = rating - num7;
            double num9 = num8 / num5;
            numArray[RatingSGDFactorizer.USER_BIAS_INDEX] += (base.biasLearningRate * currentLearningRate) * (num8 - ((base.biasReg * base.preventOverfitting) * numArray[RatingSGDFactorizer.USER_BIAS_INDEX]));
            numArray2[RatingSGDFactorizer.ITEM_BIAS_INDEX] += (base.biasLearningRate * currentLearningRate) * (num8 - ((base.biasReg * base.preventOverfitting) * numArray2[RatingSGDFactorizer.ITEM_BIAS_INDEX]));
            for (num6 = RatingSGDFactorizer.FEATURE_OFFSET; num6 < base.numFeatures; num6++)
            {
                double num10 = numArray[num6];
                double num11 = numArray2[num6];
                double num12 = (num8 * num11) - (base.preventOverfitting * num10);
                numArray[num6] += currentLearningRate * num12;
                double num13 = (num8 * userVector[num6]) - (base.preventOverfitting * num11);
                numArray2[num6] += currentLearningRate * num13;
                double num14 = num9 * num11;
                foreach (int num15 in this.itemsByUser[index])
                {
                    double num16 = num14 - (base.preventOverfitting * this.y[num15][num6]);
                    this.y[num15][num6] += base.learningRate * num16;
                }
            }
        }
    }
}