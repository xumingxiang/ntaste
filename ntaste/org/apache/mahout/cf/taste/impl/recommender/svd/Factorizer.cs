namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste.common;

    public interface Factorizer : Refreshable
    {
        Factorization factorize();
    }
}