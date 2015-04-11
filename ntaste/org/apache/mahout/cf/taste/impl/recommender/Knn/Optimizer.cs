namespace org.apache.mahout.cf.taste.impl.recommender.knn
{
    public interface Optimizer
    {
        double[] optimize(double[][] matrix, double[] b);
    }
}