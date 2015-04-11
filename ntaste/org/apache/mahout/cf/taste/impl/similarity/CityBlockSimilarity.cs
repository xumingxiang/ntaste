namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public sealed class CityBlockSimilarity : AbstractItemSimilarity, UserSimilarity, Refreshable
    {
        public CityBlockSimilarity(DataModel dataModel)
            : base(dataModel)
        {
        }

        private static double doSimilarity(int pref1, int pref2, int intersection)
        {
            int num = (pref1 + pref2) - (2 * intersection);
            return (1.0 / (1.0 + num));
        }

        public override double[] itemSimilarities(long itemID1, long[] itemID2s)
        {
            DataModel model = base.getDataModel();
            int num = model.getNumUsersWithPreferenceFor(itemID1);
            double[] numArray = new double[itemID2s.Length];
            for (int i = 0; i < itemID2s.Length; i++)
            {
                int num3 = model.getNumUsersWithPreferenceFor(itemID2s[i]);
                int intersection = model.getNumUsersWithPreferenceFor(itemID1, itemID2s[i]);
                numArray[i] = doSimilarity(num, num3, intersection);
            }
            return numArray;
        }

        public override double itemSimilarity(long itemID1, long itemID2)
        {
            DataModel model = base.getDataModel();
            int num = model.getNumUsersWithPreferenceFor(itemID1);
            int num2 = model.getNumUsersWithPreferenceFor(itemID2);
            int intersection = model.getNumUsersWithPreferenceFor(itemID1, itemID2);
            return doSimilarity(num, num2, intersection);
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            RefreshHelper.maybeRefresh(RefreshHelper.buildRefreshed(alreadyRefreshed), base.getDataModel());
        }

        public void setPreferenceInferrer(PreferenceInferrer inferrer)
        {
            throw new NotSupportedException();
        }

        public double userSimilarity(long userID1, long userID2)
        {
            DataModel model = base.getDataModel();
            FastIDSet other = model.getItemIDsFromUser(userID1);
            FastIDSet set2 = model.getItemIDsFromUser(userID2);
            int num = other.size();
            int num2 = set2.size();
            int intersection = (num < num2) ? set2.intersectionSize(other) : other.intersectionSize(set2);
            return doSimilarity(num, num2, intersection);
        }
    }
}