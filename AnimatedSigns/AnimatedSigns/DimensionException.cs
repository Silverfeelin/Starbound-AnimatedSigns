using System;

namespace AnimatedSigns
{
    public class DimensionException : Exception
    {
        public DimensionException() : base() { }
        public DimensionException(string message) : base(message) { }
    }
}
