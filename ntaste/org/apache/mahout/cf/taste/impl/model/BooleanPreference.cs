namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste.model;
    using System;

    public sealed class BooleanPreference : Preference
    {
        private long itemID;
        private long userID;

        public BooleanPreference(long userID, long itemID)
        {
            this.userID = userID;
            this.itemID = itemID;
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
            return 1f;
        }

        public void setValue(float value)
        {
            throw new NotSupportedException();
        }

        public string toString()
        {
            return string.Concat(new object[] { "BooleanPreference[userID: ", this.userID, ", itemID:", this.itemID, ']' });
        }
    }
}