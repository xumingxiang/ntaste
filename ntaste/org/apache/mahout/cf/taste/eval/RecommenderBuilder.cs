namespace org.apache.mahout.cf.taste.eval
{
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;

    public interface RecommenderBuilder
    {
        Recommender buildRecommender(DataModel dataModel);
    }
}