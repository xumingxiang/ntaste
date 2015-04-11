namespace org.apache.mahout.cf.taste.impl.common
{
    using org.apache.mahout.cf.taste;
    using System;
    using System.Linq;
    using System.Text;

    public sealed class BitSet : ICloneable
    {
        private long[] bits;

        public BitSet(int numBits)
            : this((uint)numBits)
        {
        }

        public BitSet(uint numBits)
        {
            uint num = numBits >> 6;
            if ((numBits & 0x3f) != 0)
            {
                num++;
            }
            this.bits = new long[num];
        }

        private BitSet(long[] bits)
        {
            this.bits = bits;
        }

        public void clear()
        {
            int length = this.bits.Length;
            for (int i = 0; i < length; i++)
            {
                this.bits[i] = 0L;
            }
        }

        public void clear(int index)
        {
            this.bits[index >> 6] &= ~(((long)1L) << index);
        }

        public object Clone()
        {
            return new BitSet((long[])this.bits.Clone());
        }

        public override bool Equals(object o)
        {
            if (!(o is BitSet))
            {
                return false;
            }
            BitSet set = (BitSet)o;
            return this.bits.SequenceEqual<long>(set.bits);
        }

        public bool get(int index)
        {
            return ((this.bits[index >> 6] & (((long)1L) << index)) != 0L);
        }

        public override int GetHashCode()
        {
            return Utils.GetArrayHashCode(this.bits);
        }

        public void set(int index)
        {
            this.bits[index >> 6] |= ((long)1L) << index;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x40 * this.bits.Length);
            foreach (long num in this.bits)
            {
                for (int i = 0; i < 0x40; i++)
                {
                    builder.Append(((num & (((long)1L) << i)) == 0L) ? '0' : '1');
                }
                builder.Append(' ');
            }
            return builder.ToString();
        }
    }
}