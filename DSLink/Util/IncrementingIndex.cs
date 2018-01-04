namespace DSLink.Util
{
    public class IncrementingIndex
    {
        /// <summary>
        /// Current value.
        /// </summary>
        public int Current
        {
            get;
            private set;
        }

        /// <summary>
        /// Return the Current value, and increment it afterwards.
        /// </summary>
        public int Next
        {
            get
            {
                return Current++;
            }
        }

        public IncrementingIndex()
        {
            Current = 0;
        }

        public IncrementingIndex(int startingValue)
        {
            Current = 1;
        }
    }
}
