namespace org.apache.mahout.cf.taste
{
    public class AtomicInteger
    {
        private int Value;

        public AtomicInteger()
        {
            this.Value = 0;
        }

        public AtomicInteger(int val)
        {
            this.Value = val;
        }

        public int get()
        {
            return this.Value;
        }

        public int incrementAndGet()
        {
            lock (this)
            {
                return ++this.Value;
            }
        }
    }
}