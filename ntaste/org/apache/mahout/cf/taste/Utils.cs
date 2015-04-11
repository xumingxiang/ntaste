namespace org.apache.mahout.cf.taste
{
    using System;

    public static class Utils
    {
        public static bool ArrayDeepEquals(Array arr1, Array arr2)
        {
            if ((arr1.Length != arr2.Length) || (arr1.GetType() != arr2.GetType()))
            {
                return false;
            }
            for (int i = 0; i < arr1.Length; i++)
            {
                object obj2 = arr1.GetValue(i);
                object obj3 = arr2.GetValue(i);
                if ((obj2 is Array) && (obj3 is Array))
                {
                    if (!ArrayDeepEquals((Array)obj2, (Array)obj3))
                    {
                        return false;
                    }
                }
                else if ((obj2 != null) || (obj3 != null))
                {
                    if ((obj2 != null) && !obj2.Equals(obj3))
                    {
                        return false;
                    }
                    if ((obj3 != null) && !obj3.Equals(obj2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static int GetArrayDeepHashCode(Array arr)
        {
            int length = arr.Length;
            for (int i = 0; i < arr.Length; i++)
            {
                object obj2 = arr.GetValue(i);
                int num3 = (obj2 is Array) ? GetArrayDeepHashCode((Array)obj2) : obj2.GetHashCode();
                length ^= num3;
            }
            return length;
        }

        public static int GetArrayHashCode(Array arr)
        {
            int length = arr.Length;
            for (int i = 0; i < arr.Length; i++)
            {
                length ^= arr.GetValue(i).GetHashCode();
            }
            return length;
        }
    }
}