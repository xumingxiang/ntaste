namespace org.apache.mahout.cf.taste.eval
{
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;

    public interface RecommenderIRStatsEvaluator
    {
        IRStatistics evaluate(RecommenderBuilder recommenderBuilder, DataModelBuilder dataModelBuilder, DataModel dataModel, IDRescorer rescorer, int at, double relevanceThreshold, double evaluationPercentage);
    }
}