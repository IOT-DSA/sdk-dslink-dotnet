using System;

namespace DSLink
{
    public class DSAException : Exception
    {
        public readonly DSLinkContainer Container;
        
        public DSAException(DSLinkContainer container, string message) : base(message)
        {
            Container = container;
        }
    }
}