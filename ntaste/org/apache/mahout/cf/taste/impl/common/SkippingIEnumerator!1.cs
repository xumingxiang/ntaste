namespace org.apache.mahout.cf.taste.impl.common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface SkippingIEnumerator<V> : IEnumerator<V>, IDisposable, IEnumerator
    {
    }
}