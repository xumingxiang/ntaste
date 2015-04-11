namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    public class NoPersistenceStrategy : PersistenceStrategy
    {
        public Factorization load()
        {
            return null;
        }

        public void maybePersist(Factorization factorization)
        {
        }
    }
}