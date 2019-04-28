namespace DSLink.Util
{
    /// <summary>
    /// This class implements an auto-incrementing integer.
    /// </summary>
    public class IncrementingIndex
    {
        /// <summary>
        /// Fetch the current value without modifying the value.
        /// </summary>
        public int Current
        {
            get;
            private set;
        }

        /// <summary>
        /// Fetch the current value, and post-increment it.
        /// </summary>
        public int CurrentAndIncrement => Current++;

        /// <summary>
        /// Default constructor which initializes the value to zero.
        /// </summary>
        public IncrementingIndex()
        {
            Current = 0;
        }

        /// <summary>
        /// Constructor that will initialize the value to the given parameter.
        /// </summary>
        /// <param name="startingValue">Starting value to initialize this value to</param>
        public IncrementingIndex(int startingValue)
        {
            Current = startingValue;
        }
    }
}