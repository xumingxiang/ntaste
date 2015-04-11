namespace org.apache.mahout.cf.taste
{
    using System;

    public class Logger
    {
        private Type LogType;

        public Logger(Type t)
        {
            this.LogType = t;
        }

        public void debug(string format, params object[] args)
        {
        }

        public void info(string format, params object[] args)
        {
        }

        public void warn(string format, params object[] args)
        {
        }
    }
}