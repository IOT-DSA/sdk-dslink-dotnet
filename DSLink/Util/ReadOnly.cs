using System;

namespace DSLink.Util
{
    public class ReadOnlyException : Exception
    {
        public ReadOnlyException() : base("Dictionary is read-only.")
        {
        }
    }
}
