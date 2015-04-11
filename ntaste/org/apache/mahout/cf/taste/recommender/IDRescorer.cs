namespace org.apache.mahout.cf.taste.recommender
{
    public interface IDRescorer
    {
        bool isFiltered(long id);

        double rescore(long id, double originalScore);
    }
}