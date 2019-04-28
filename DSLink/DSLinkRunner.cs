namespace DSLink
{
    public class DSLinkRunner
    {
        private readonly string[] _args;
        private readonly BaseLinkHandler _handler;

        private DSLinkRunner(string[] args, BaseLinkHandler handler)
        {
            _args = args;
            _handler = handler;
        }

        public void Run()
        {
            _handler.Connect();
        }

        public static void Start(string[] args, BaseLinkHandler handler)
        {
            var runner = new DSLinkRunner(args, handler);
            runner.Run();
        }

        public static DSLinkRunner Create(string[] args, BaseLinkHandler handler)
        {
            return new DSLinkRunner(args, handler);
        }
    }
}