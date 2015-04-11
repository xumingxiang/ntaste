namespace org.apache.mahout.cf.taste.eval
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;

    public interface DataModelBuilder
    {
        DataModel buildDataModel(FastByIDMap<PreferenceArray> trainingData);
    }
}