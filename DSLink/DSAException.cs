using System;

namespace DSLink
{
    public class DSAException : Exception
    {
        public readonly BaseLinkHandler Handler;

        public DSAException(BaseLinkHandler handler, string message) : base(message)
        {
            Handler = handler;
        }
    }
}