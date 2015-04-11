namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.neighborhood;
    using org.apache.mahout.cf.taste.similarity;

    public sealed class GenericBooleanPrefUserBasedRecommender : GenericUserBasedRecommender
    {
        public GenericBooleanPrefUserBasedRecommender(DataModel dataModel, UserNeighborhood neighborhood, UserSimilarity similarity)
            : base(dataModel, neighborhood, similarity)
        {
        }

        protected override float doEstimatePreference(long theUserID, long[] theNeighborhood, long itemID)
        {
            if (theNeighborhood.Length == 0)
            {
                return float.NaN;
            }
            DataModel model = this.getDataModel();
            UserSimilarity similarity = base.getSimilarity();
            float num = 0f;
            bool flag = false;
            foreach (long num2 in theNeighborhood)
            {
                if ((num2 != theUserID) && model.getPreferenceValue(num2, itemID).HasValue)
                {
                    flag = true;
                    num += (float)similarity.userSimilarity(theUserID, num2);
                }
            }
            return (flag ? num : float.NaN);
        }

        protected FastIDSet getAllOtherItems(long[] theNeighborhood, long theUserID)
        {
            DataModel model = this.getDataModel();
            FastIDSet set = new FastIDSet();
            foreach (long num in theNeighborhood)
            {
                set.addAll(model.getItemIDsFromUser(num));
            }
            set.removeAll(model.getItemIDsFromUser(theUserID));
            return set;
        }

        public override string ToString()
        {
            return "GenericBooleanPrefUserBasedRecommender";
        }
    }
}