using Console = System.Diagnostics.Debug;

namespace DSLink.Util.Logger
{
    public class Logger
    {
        public string Name { get; }

        public Logger(string name)
        {
            Name = name;
        }

        public void Info(string message)
        {
            Console.WriteLine("[INFO] {0}", message);
        }

        public void Warn(string message)
        {
            Console.WriteLine("[WARN] {0}", message);
        }

        public void Debug(string message)
        {
            Console.WriteLine("[DEBUG] {0}", message);
        }
    }
}
