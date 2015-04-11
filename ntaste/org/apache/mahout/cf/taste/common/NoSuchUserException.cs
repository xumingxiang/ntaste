namespace org.apache.mahout.cf.taste.common
{
    public sealed class NoSuchUserException : TasteException
    {
        public NoSuchUserException()
        {
        }

        public NoSuchUserException(long userID)
            : this(string.Format("No such user: {0}", userID))
        {
        }

        public NoSuchUserException(string message)
            : base(message)
        {
        }
    }
}