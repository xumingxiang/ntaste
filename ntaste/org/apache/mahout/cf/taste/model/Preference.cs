namespace org.apache.mahout.cf.taste.model
{
    public interface Preference
    {
        long getItemID();

        long getUserID();

        float getValue();

        void setValue(float value);
    }
}