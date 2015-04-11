namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.recommender;

    public sealed class NullRescorer<T> : Rescorer<T>, IDRescorer
    {
        internal NullRescorer()
        {
        }

        public bool isFiltered(T thing)
        {
            return false;
        }

        public bool isFiltered(long id)
        {
            return false;
        }

        public double rescore(T thing, double originalScore)
        {
            return originalScore;
        }

        public double rescore(long id, double originalScore)
        {
            return originalScore;
        }

        public override string ToString()
        {
            return "NullRescorer";
        }
    }
}