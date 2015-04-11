namespace org.apache.mahout.cf.taste
{
    using System;

    public static class LoggerFactory
    {
        public static Logger getLogger(Type t)
        {
            return new Logger(t);
        }
    }
}