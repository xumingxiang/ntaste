namespace org.apache.mahout.cf.taste.impl.common
{
    public interface Retriever<K, V>
    {
        V get(K key);
    }
}