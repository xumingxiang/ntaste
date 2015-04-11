namespace org.apache.mahout.cf.taste.recommender
{
    public interface Rescorer<T>
    {
        bool isFiltered(T thing);

        double rescore(T thing, double originalScore);
    }
}