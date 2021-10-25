using System;

namespace meerkat.Exceptions
{
    public sealed class InvalidAttributeException : Exception
    {
        public InvalidAttributeException(string exceptionMessage) : base(exceptionMessage)
        {
        }
    }
}