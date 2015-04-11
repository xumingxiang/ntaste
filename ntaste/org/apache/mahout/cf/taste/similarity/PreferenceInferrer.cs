namespace org.apache.mahout.cf.taste.similarity
{
    using org.apache.mahout.cf.taste.common;

    public interface PreferenceInferrer : Refreshable
    {
        float inferPreference(long userID, long itemID);
    }
}