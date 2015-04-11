namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public sealed class TanimotoCoefficientSimilarity : AbstractItemSimilarity, UserSimilarity, Refreshable
    {
        public TanimotoCoefficientSimilarity(DataModel dataModel)
            : base(dataModel)
        {
        }

        private double doItemSimilarity(long itemID1, long itemID2, int preferring1)
        {
            DataModel model = base.getDataModel();
            int num = model.getNumUsersWithPreferenceFor(itemID1, itemID2);
            if (num == 0)
            {
                return double.NaN;
            }
            int num2 = model.getNumUsersWithPreferenceFor(itemID2);
            return (((double)num) / ((double)((preferring1 + num2) - num)));
        }

        public override double[] itemSimilarities(long itemID1, long[] itemID2s)
        {
            int num = base.getDataModel().getNumUsersWithPreferenceFor(itemID1);
            int length = itemID2s.Length;
            double[] numArray = new double[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = this.doItemSimilarity(itemID1, itemID2s[i], num);
            }
            return numArray;
        }

        public override double itemSimilarity(long itemID1, long itemID2)
        {
            int num = base.getDataModel().getNumUsersWithPreferenceFor(itemID1);
            return this.doItemSimilarity(itemID1, itemID2, num);
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            alreadyRefreshed = RefreshHelper.buildRefreshed(alreadyRefreshed);
            RefreshHelper.maybeRefresh(alreadyRefreshed, base.getDataModel());
        }

        public void setPreferenceInferrer(PreferenceInferrer inferrer)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return ("TanimotoCoefficientSimilarity[dataModel:" + base.getDataModel() + ']');
        }

        public double userSimilarity(long userID1, long userID2)
        {
            DataModel model = base.getDataModel();
            FastIDSet other = model.getItemIDsFromUser(userID1);
            FastIDSet set2 = model.getItemIDsFromUser(userID2);
            int num = other.size();
            int num2 = set2.size();
            if ((num == 0) && (num2 == 0))
            {
                return double.NaN;
            }
            if ((num == 0) || (num2 == 0))
            {
                return 0.0;
            }
            int num3 = (num < num2) ? set2.intersectionSize(other) : other.intersectionSize(set2);
            if (num3 == 0)
            {
                return double.NaN;
            }
            int num4 = (num + num2) - num3;
            return (((double)num3) / ((double)num4));
        }
    }
}