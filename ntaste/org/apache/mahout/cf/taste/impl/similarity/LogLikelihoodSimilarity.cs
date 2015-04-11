namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using org.apache.mahout.math.stats;
    using System;
    using System.Collections.Generic;

    public sealed class LogLikelihoodSimilarity : AbstractItemSimilarity, UserSimilarity, Refreshable
    {
        public LogLikelihoodSimilarity(DataModel dataModel)
            : base(dataModel)
        {
        }

        private double doItemSimilarity(long itemID1, long itemID2, long preferring1, long numUsers)
        {
            DataModel model = base.getDataModel();
            long num = model.getNumUsersWithPreferenceFor(itemID1, itemID2);
            if (num == 0L)
            {
                return double.NaN;
            }
            long num2 = model.getNumUsersWithPreferenceFor(itemID2);
            double num3 = LogLikelihood.logLikelihoodRatio(num, num2 - num, preferring1 - num, ((numUsers - preferring1) - num2) + num);
            return (1.0 - (1.0 / (1.0 + num3)));
        }

        public override double[] itemSimilarities(long itemID1, long[] itemID2s)
        {
            DataModel model = base.getDataModel();
            long num = model.getNumUsersWithPreferenceFor(itemID1);
            long numUsers = model.getNumUsers();
            int length = itemID2s.Length;
            double[] numArray = new double[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = this.doItemSimilarity(itemID1, itemID2s[i], num, numUsers);
            }
            return numArray;
        }

        public override double itemSimilarity(long itemID1, long itemID2)
        {
            DataModel model = base.getDataModel();
            long num = model.getNumUsersWithPreferenceFor(itemID1);
            long numUsers = model.getNumUsers();
            return this.doItemSimilarity(itemID1, itemID2, num, numUsers);
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
            return ("LogLikelihoodSimilarity[dataModel:" + base.getDataModel() + ']');
        }

        public double userSimilarity(long userID1, long userID2)
        {
            DataModel model = base.getDataModel();
            FastIDSet other = model.getItemIDsFromUser(userID1);
            FastIDSet set2 = model.getItemIDsFromUser(userID2);
            long num = other.size();
            long num2 = set2.size();
            long num3 = (num < num2) ? ((long)set2.intersectionSize(other)) : ((long)other.intersectionSize(set2));
            if (num3 == 0L)
            {
                return double.NaN;
            }
            long num4 = model.getNumItems();
            double num5 = LogLikelihood.logLikelihoodRatio(num3, num2 - num3, num - num3, ((num4 - num) - num2) + num3);
            return (1.0 - (1.0 / (1.0 + num5)));
        }
    }
}