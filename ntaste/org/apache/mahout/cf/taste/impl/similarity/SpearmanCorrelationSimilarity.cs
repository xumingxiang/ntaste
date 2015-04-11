namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public sealed class SpearmanCorrelationSimilarity : UserSimilarity, Refreshable
    {
        private DataModel dataModel;

        public SpearmanCorrelationSimilarity(DataModel dataModel)
        {
            this.dataModel = dataModel;
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            alreadyRefreshed = RefreshHelper.buildRefreshed(alreadyRefreshed);
            RefreshHelper.maybeRefresh(alreadyRefreshed, this.dataModel);
        }

        public void setPreferenceInferrer(PreferenceInferrer inferrer)
        {
            throw new NotSupportedException();
        }

        public double userSimilarity(long userID1, long userID2)
        {
            int num4;
            PreferenceArray array = this.dataModel.getPreferencesFromUser(userID1);
            PreferenceArray array2 = this.dataModel.getPreferencesFromUser(userID2);
            int num = array.length();
            int num2 = array2.length();
            if ((num <= 1) || (num2 <= 1))
            {
                return double.NaN;
            }
            array = array.clone();
            array2 = array2.clone();
            array.sortByValue();
            array2.sortByValue();
            float num3 = 1f;
            for (num4 = 0; num4 < num; num4++)
            {
                if (array2.hasPrefWithItemID(array.getItemID(num4)))
                {
                    array.setValue(num4, num3);
                    num3++;
                }
            }
            num3 = 1f;
            for (num4 = 0; num4 < num2; num4++)
            {
                if (array.hasPrefWithItemID(array2.getItemID(num4)))
                {
                    array2.setValue(num4, num3);
                    num3++;
                }
            }
            array.sortByItem();
            array2.sortByItem();
            long num5 = array.getItemID(0);
            long num6 = array2.getItemID(0);
            int i = 0;
            int num8 = 0;
            double num9 = 0.0;
            int num10 = 0;
            while (true)
            {
                int num11 = (num5 < num6) ? -1 : ((num5 > num6) ? 1 : 0);
                if (num11 == 0)
                {
                    double num12 = array.getValue(i) - array2.getValue(num8);
                    num9 += num12 * num12;
                    num10++;
                }
                if (num11 <= 0)
                {
                    if (++i >= num)
                    {
                        break;
                    }
                    num5 = array.getItemID(i);
                }
                if (num11 >= 0)
                {
                    if (++num8 >= num2)
                    {
                        break;
                    }
                    num6 = array2.getItemID(num8);
                }
            }
            if (num10 <= 1)
            {
                return double.NaN;
            }
            return (1.0 - ((6.0 * num9) / ((double)(num10 * ((num10 * num10) - 1)))));
        }
    }
}