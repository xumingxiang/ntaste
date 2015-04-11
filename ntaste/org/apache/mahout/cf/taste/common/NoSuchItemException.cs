namespace org.apache.mahout.cf.taste.common
{
    public sealed class NoSuchItemException : TasteException
    {
        public NoSuchItemException()
        {
        }

        public NoSuchItemException(long itemID)
            : this(itemID.ToString())
        {
        }

        public NoSuchItemException(string message)
            : base(message)
        {
        }
    }
}