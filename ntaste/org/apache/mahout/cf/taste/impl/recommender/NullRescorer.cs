namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.recommender;
    using System;

    public static class NullRescorer
    {
        private static Rescorer<Tuple<long, long>> ITEM_ITEM_PAIR_INSTANCE = new NullRescorer<Tuple<long, long>>();
        private static IDRescorer USER_OR_ITEM_INSTANCE = new NullRescorer<long>();
        private static Rescorer<Tuple<long, long>> USER_USER_PAIR_INSTANCE = new NullRescorer<Tuple<long, long>>();

        public static IDRescorer getItemInstance()
        {
            return USER_OR_ITEM_INSTANCE;
        }

        public static Rescorer<Tuple<long, long>> getItemItemPairInstance()
        {
            return ITEM_ITEM_PAIR_INSTANCE;
        }

        public static IDRescorer getUserInstance()
        {
            return USER_OR_ITEM_INSTANCE;
        }

        public static Rescorer<Tuple<long, long>> getUserUserPairInstance()
        {
            return USER_USER_PAIR_INSTANCE;
        }
    }
}