namespace org.apache.mahout.cf.taste.eval
{
    using org.apache.mahout.cf.taste.model;
    using System;

    public interface RecommenderEvaluator
    {
        double evaluate(RecommenderBuilder recommenderBuilder, DataModelBuilder dataModelBuilder, DataModel dataModel, double trainingPercentage, double evaluationPercentage);

        [Obsolete]
        float getMaxPreference();

        [Obsolete]
        float getMinPreference();

        [Obsolete]
        void setMaxPreference(float maxPreference);

        [Obsolete]
        void setMinPreference(float minPreference);
    }
}