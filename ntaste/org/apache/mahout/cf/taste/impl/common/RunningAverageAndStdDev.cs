namespace org.apache.mahout.cf.taste.impl.common
{
    public interface RunningAverageAndStdDev : RunningAverage
    {
        double getStandardDeviation();

        new RunningAverageAndStdDev inverse();
    }
}