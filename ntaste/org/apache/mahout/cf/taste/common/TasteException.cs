namespace org.apache.mahout.cf.taste.common
{
    using System;

    public class TasteException : Exception
    {
        public TasteException()
        {
        }

        public TasteException(string message)
            : base(message)
        {
        }

        public TasteException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}