using System;

namespace VideoARDemo
{
    internal class Record<T>
    {
        public DateTime Time;
        public bool IsKey;
        public T Package;
        public byte[] Header;
    }
}