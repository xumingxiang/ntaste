namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.recommender;
    using org.apache.mahout.common;
    using System;

    [Serializable]
    public sealed class GenericRecommendedItem : RecommendedItem
    {
        private long itemID;
        private float value;

        public GenericRecommendedItem(long itemID, float value)
        {
            this.itemID = itemID;
            this.value = value;
        }

        public override bool Equals(object o)
        {
            if (!(o is GenericRecommendedItem))
            {
                return false;
            }
            RecommendedItem item = (RecommendedItem)o;
            return ((this.itemID == item.getItemID()) && (this.value == item.getValue()));
        }

        public override int GetHashCode()
        {
            return (((int)this.itemID) ^ RandomUtils.hashFloat(this.value));
        }

        public long getItemID()
        {
            return this.itemID;
        }

        public float getValue()
        {
            return this.value;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "RecommendedItem[item:", this.itemID, ", value:", this.value, ']' });
        }
    }
}