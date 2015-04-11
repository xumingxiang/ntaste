namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using org.apache.mahout.cf.taste.similarity;

    public sealed class GenericBooleanPrefItemBasedRecommender : GenericItemBasedRecommender
    {
        public GenericBooleanPrefItemBasedRecommender(DataModel dataModel, ItemSimilarity similarity)
            : base(dataModel, similarity)
        {
        }

        public GenericBooleanPrefItemBasedRecommender(DataModel dataModel, ItemSimilarity similarity, CandidateItemsStrategy candidateItemsStrategy, MostSimilarItemsCandidateItemsStrategy mostSimilarItemsCandidateItemsStrategy)
            : base(dataModel, similarity, candidateItemsStrategy, mostSimilarItemsCandidateItemsStrategy)
        {
        }

        protected override float doEstimatePreference(long userID, PreferenceArray preferencesFromUser, long itemID)
        {
            double[] numArray = base.getSimilarity().itemSimilarities(itemID, preferencesFromUser.getIDs());
            bool flag = false;
            double num = 0.0;
            foreach (double num2 in numArray)
            {
                if (!double.IsNaN(num2))
                {
                    flag = true;
                    num += num2;
                }
            }
            return (flag ? ((float)num) : float.NaN);
        }

        public override string ToString()
        {
            return "GenericBooleanPrefItemBasedRecommender";
        }
    }
}