namespace org.apache.mahout.cf.taste.eval
{
    public interface IRStatistics
    {
        double getF1Measure();

        double getFallOut();

        double getFNMeasure(double n);

        double getNormalizedDiscountedCumulativeGain();

        double getPrecision();

        double getReach();

        double getRecall();
    }
}