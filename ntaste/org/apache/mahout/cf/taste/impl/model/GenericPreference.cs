namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste.model;
    using System;

    [Serializable]
    public class GenericPreference : Preference
    {
        private long itemID;
        private long userID;
        private float value;

        public GenericPreference(long userID, long itemID, float value)
        {
            this.userID = userID;
            this.itemID = itemID;
            this.value = value;
        }

        public long getItemID()
        {
            return this.itemID;
        }

        public long getUserID()
        {
            return this.userID;
        }

        public float getValue()
        {
            return this.value;
        }

        public void setValue(float value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "GenericPreference[userID: ", this.userID, ", itemID:", this.itemID, ", value:", this.value, ']' });
        }
    }
}