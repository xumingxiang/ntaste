namespace org.apache.commons.math3.exception
{
    using System;

    public class NotStrictlyPositiveException : ArgumentException
    {
        public NotStrictlyPositiveException(object value)
            : base(string.Format("Argument is not positive: {0}", value))
        {
        }
    }
}