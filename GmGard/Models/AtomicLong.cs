using System.Threading;

namespace GmGard.Models
{
    public class AtomicLong
    {
        private long value;

        public AtomicLong(long v)
        {
            value = v;
        }

        public long Value
        {
            get { return value; }
        }

        public void Increment()
        {
            Interlocked.Increment(ref value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref value);
        }
    }
}