namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public abstract class AbstractSimilarity : AbstractItemSimilarity, UserSimilarity, Refreshable
    {
        private int cachedNumItems;
        private int cachedNumUsers;
        private bool centerData;
        private PreferenceInferrer inferrer;
        private RefreshHelper refreshHelper;
        private bool weighted;

        public AbstractSimilarity(DataModel dataModel, Weighting weighting, bool centerData)
            : base(dataModel)
        {
            Action refreshRunnable = null;
            this.weighted = weighting == Weighting.WEIGHTED;
            this.centerData = centerData;
            this.cachedNumItems = dataModel.getNumItems();
            this.cachedNumUsers = dataModel.getNumUsers();
            if (refreshRunnable == null)
            {
                refreshRunnable = delegate
                {
                    this.cachedNumItems = dataModel.getNumItems();
                    this.cachedNumUsers = dataModel.getNumUsers();
                };
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
        }

        protected abstract double computeResult(int n, double sumXY, double sumX2, double sumY2, double sumXYdiff2);

        private PreferenceInferrer getPreferenceInferrer()
        {
            return this.inferrer;
        }

        private bool isWeighted()
        {
            return this.weighted;
        }

        public override double[] itemSimilarities(long itemID1, long[] itemID2s)
        {
            int length = itemID2s.Length;
            double[] numArray = new double[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = this.itemSimilarity(itemID1, itemID2s[i]);
            }
            return numArray;
        }

        public override double itemSimilarity(long itemID1, long itemID2)
        {
            double num18;
            DataModel model = base.getDataModel();
            PreferenceArray array = model.getPreferencesForItem(itemID1);
            PreferenceArray array2 = model.getPreferencesForItem(itemID2);
            int num = array.length();
            int num2 = array2.length();
            if ((num == 0) || (num2 == 0))
            {
                return double.NaN;
            }
            long num3 = array.getUserID(0);
            long num4 = array2.getUserID(0);
            int i = 0;
            int num6 = 0;
            double num7 = 0.0;
            double num8 = 0.0;
            double num9 = 0.0;
            double num10 = 0.0;
            double sumXY = 0.0;
            double num12 = 0.0;
            int n = 0;
            while (true)
            {
                int num14 = (num3 < num4) ? -1 : ((num3 > num4) ? 1 : 0);
                if (num14 == 0)
                {
                    double num15 = array.getValue(i);
                    double num16 = array2.getValue(num6);
                    sumXY += num15 * num16;
                    num7 += num15;
                    num8 += num15 * num15;
                    num9 += num16;
                    num10 += num16 * num16;
                    double num17 = num15 - num16;
                    num12 += num17 * num17;
                    n++;
                }
                if (num14 <= 0)
                {
                    if (++i == num)
                    {
                        break;
                    }
                    num3 = array.getUserID(i);
                }
                if (num14 >= 0)
                {
                    if (++num6 == num2)
                    {
                        break;
                    }
                    num4 = array2.getUserID(num6);
                }
            }
            if (this.centerData)
            {
                double num19 = n;
                double num20 = num7 / num19;
                double num21 = num9 / num19;
                double num22 = sumXY - (num21 * num7);
                double num23 = num8 - (num20 * num7);
                double num24 = num10 - (num21 * num9);
                num18 = this.computeResult(n, num22, num23, num24, num12);
            }
            else
            {
                num18 = this.computeResult(n, sumXY, num8, num10, num12);
            }
            if (!double.IsNaN(num18))
            {
                num18 = this.normalizeWeightResult(num18, n, this.cachedNumUsers);
            }
            return num18;
        }

        private double normalizeWeightResult(double result, int count, int num)
        {
            double num2 = result;
            if (this.weighted)
            {
                double num3 = 1.0 - (((double)count) / ((double)(num + 1)));
                if (num2 < 0.0)
                {
                    num2 = -1.0 + (num3 * (1.0 + num2));
                }
                else
                {
                    num2 = 1.0 - (num3 * (1.0 - num2));
                }
            }
            if (num2 < -1.0)
            {
                return -1.0;
            }
            if (num2 > 1.0)
            {
                num2 = 1.0;
            }
            return num2;
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            base.refresh(alreadyRefreshed);
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        public void setPreferenceInferrer(PreferenceInferrer inferrer)
        {
            this.refreshHelper.addDependency(inferrer);
            this.refreshHelper.removeDependency(this.inferrer);
            this.inferrer = inferrer;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { base.GetType().Name, "[dataModel:", base.getDataModel(), ",inferrer:", this.inferrer, ']' });
        }

        public double userSimilarity(long userID1, long userID2)
        {
            double num18;
            DataModel model = base.getDataModel();
            PreferenceArray array = model.getPreferencesFromUser(userID1);
            PreferenceArray array2 = model.getPreferencesFromUser(userID2);
            int num = array.length();
            int num2 = array2.length();
            if ((num == 0) || (num2 == 0))
            {
                return double.NaN;
            }
            long itemID = array.getItemID(0);
            long num4 = array2.getItemID(0);
            int i = 0;
            int num6 = 0;
            double num7 = 0.0;
            double num8 = 0.0;
            double num9 = 0.0;
            double num10 = 0.0;
            double sumXY = 0.0;
            double num12 = 0.0;
            int n = 0;
            bool flag = this.inferrer != null;
            while (true)
            {
                int num14 = (itemID < num4) ? -1 : ((itemID > num4) ? 1 : 0);
                if (flag || (num14 == 0))
                {
                    double num15;
                    double num16;
                    if (itemID == num4)
                    {
                        num15 = array.getValue(i);
                        num16 = array2.getValue(num6);
                    }
                    else if (num14 < 0)
                    {
                        num15 = array.getValue(i);
                        num16 = this.inferrer.inferPreference(userID2, itemID);
                    }
                    else
                    {
                        num15 = this.inferrer.inferPreference(userID1, num4);
                        num16 = array2.getValue(num6);
                    }
                    sumXY += num15 * num16;
                    num7 += num15;
                    num8 += num15 * num15;
                    num9 += num16;
                    num10 += num16 * num16;
                    double num17 = num15 - num16;
                    num12 += num17 * num17;
                    n++;
                }
                if (num14 <= 0)
                {
                    if (++i >= num)
                    {
                        if (!flag || (num4 == 0x7fffffffffffffffL))
                        {
                            break;
                        }
                        itemID = 0x7fffffffffffffffL;
                    }
                    else
                    {
                        itemID = array.getItemID(i);
                    }
                }
                if (num14 >= 0)
                {
                    if (++num6 >= num2)
                    {
                        if (!flag || (itemID == 0x7fffffffffffffffL))
                        {
                            break;
                        }
                        num4 = 0x7fffffffffffffffL;
                    }
                    else
                    {
                        num4 = array2.getItemID(num6);
                    }
                }
            }
            if (this.centerData)
            {
                double num19 = num7 / ((double)n);
                double num20 = num9 / ((double)n);
                double num21 = sumXY - (num20 * num7);
                double num22 = num8 - (num19 * num7);
                double num23 = num10 - (num20 * num9);
                num18 = this.computeResult(n, num21, num22, num23, num12);
            }
            else
            {
                num18 = this.computeResult(n, sumXY, num8, num10, num12);
            }
            if (!double.IsNaN(num18))
            {
                num18 = this.normalizeWeightResult(num18, n, this.cachedNumItems);
            }
            return num18;
        }
    }
}