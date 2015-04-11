namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    public interface PersistenceStrategy
    {
        Factorization load();

        void maybePersist(Factorization factorization);
    }
}