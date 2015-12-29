using System;
using DSLink.Connection;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;

namespace DSLink.Container
{
    public abstract class AbstractContainer
    {
        public Configuration Config { get; protected set; }
        public ILog Logger { get; private set; }
        public Connector Connector { get; protected set; }
        private readonly Responder _responder;
        private readonly Requester _requester;
        private int _msg;
        private int _rid;
        public int MessageId => _msg++;
        public int RequestId => ++_rid;

        protected AbstractContainer(Configuration config)
        {
            Config = config;

            if (Config.Responder)
            {
                _responder = new Responder(this);
            }
            if (Config.Requester)
            {
                _requester = new Requester(this);
            }
        }

        public Responder Responder
        {
            get
            {
                if (!Config.Responder)
                {
                    throw new ArgumentException("Responder is not enabled.");
                }
                return _responder;
            }
        }

        public Requester Requester
        {
            get
            {
                if (!Config.Requester)
                {
                    throw new ArgumentException("Requester is not enabled.");
                }
                return _requester;
            }
        }

        public void CreateLogger(string loggerName)
        {
            Logger = LogManager.GetLogger(loggerName);
            ConfigureLogger();
        }

        private static void ConfigureLogger()
        {
            LayoutSkeleton layout = new PatternLayout("%date|%logger|%level|%message%newline");
            layout.ActivateOptions();

            IAppender appender = new ConsoleAppender
            {
                Threshold = Level.Debug,
                Layout = layout
            };

            BasicConfigurator.Configure(appender);
        }
    }
}
