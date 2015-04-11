namespace org.apache.mahout.cf.taste.common
{
    using System.Collections.Generic;

    public interface Refreshable
    {
        void refresh(IList<Refreshable> alreadyRefreshed);
    }
}