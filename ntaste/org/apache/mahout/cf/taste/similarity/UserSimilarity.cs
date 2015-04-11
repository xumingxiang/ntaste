namespace org.apache.mahout.cf.taste.similarity
{
    using org.apache.mahout.cf.taste.common;

    public interface UserSimilarity : Refreshable
    {
        void setPreferenceInferrer(PreferenceInferrer inferrer);

        double userSimilarity(long userID1, long userID2);
    }
}