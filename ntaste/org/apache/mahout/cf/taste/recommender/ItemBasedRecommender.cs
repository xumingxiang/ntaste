namespace org.apache.mahout.cf.taste.recommender
{
    using org.apache.mahout.cf.taste.common;
    using System;
    using System.Collections.Generic;

    public interface ItemBasedRecommender : Recommender, Refreshable
    {
        List<RecommendedItem> mostSimilarItems(long itemID, int howMany);

        List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany);

        List<RecommendedItem> mostSimilarItems(long itemID, int howMany, Rescorer<Tuple<long, long>> rescorer);

        List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany, Rescorer<Tuple<long, long>> rescorer);

        List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany, bool excludeItemIfNotSimilarToAll);

        List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany, Rescorer<Tuple<long, long>> rescorer, bool excludeItemIfNotSimilarToAll);

        List<RecommendedItem> recommendedBecause(long userID, long itemID, int howMany);
    }
}