namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste.recommender;

    public sealed class LoadCallable
    {
        private Recommender recommender;
        private long userID;

        public LoadCallable(Recommender recommender, long userID)
        {
            this.recommender = recommender;
            this.userID = userID;
        }

        public void call()
        {
            this.recommender.recommend(this.userID, 10);
        }
    }
}