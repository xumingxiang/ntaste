namespace org.apache.mahout.cf.taste.impl.common
{
    public interface RunningAverage
    {
        void addDatum(double datum);

        void changeDatum(double delta);

        double getAverage();

        int getCount();

        RunningAverage inverse();

        void removeDatum(double datum);
    }
}