using System;
using System.Threading;
using System.Threading.Tasks;
using DSLink.Nodes;
using DSLink.Respond;
using Newtonsoft.Json.Linq;
using Serilog;
using CommandLine;
using System.IO;

namespace DSLink.Example.Requester
{
    public class ExampleRequesterDSLink : DSLinkContainer
    {
        public static void Main(string[] args)
        {
            var results = Parser.Default.ParseArguments<CommandLineArguments>(args)
                 .WithParsed(cmdLineOptions =>
                 {
                     if (cmdLineOptions.LogFileFolder != null && !cmdLineOptions.LogFileFolder.EndsWith(Path.DirectorySeparatorChar)) {
                         throw new ArgumentException($"Specified LogFileFolder must end with '{Path.DirectorySeparatorChar}'");
                     }

                     //Init the logging engine
                     InitializeLogging(cmdLineOptions);

                     //Construct a link Configuration
                     var config = new Configuration(cmdLineOptions.LinkName, true, true);

                     //Construct our custom link
                     var dslink = new ExampleRequesterDSLink(config, cmdLineOptions);

                     InitializeLink(dslink).Wait();
                 })
                 .WithNotParsed(errors =>
                 {
                     Environment.Exit(-1);
                 });


            while (true) {
                Thread.Sleep(1000);
            }
        }

        public static async Task InitializeLink(ExampleRequesterDSLink dsLink)
        {
            await dsLink.Connect();
        }

        public ExampleRequesterDSLink(Configuration config, CommandLineArguments cmdLineOptions) : base(config)
        {
            //Perform any configuration overrides from comman line options
            if (cmdLineOptions.BrokerUrl != null) {
                config.BrokerUrl = cmdLineOptions.BrokerUrl;
            }
            if (cmdLineOptions.Token != null) {
                config.Token = cmdLineOptions.Token;
            }
            if (cmdLineOptions.NodesFileName != null) {
                config.NodesFilename = cmdLineOptions.NodesFileName;
            }

            config.LoadNodesJson = false;
        }

        protected override void OnConnectionOpen()
        {
            base.OnConnectionOpen();

            Requester.Invoke("/downstream/DQL/query", Permission.Read, new JObject
            {
                new JProperty("query", "list /downstream/?")
            }, dqlCallback);
        }

        private void dqlCallback(InvokeResponse invokeResponse)
        {
            foreach (var invokeResponseUpdate in invokeResponse.Updates) {
                Console.WriteLine(invokeResponseUpdate.ToString());
            }
        }
        

        /// <summary>
        /// This method initializes the logging engine.  In this case Serilog is used, but you
        /// may use a variety of logging engines so long as they are compatible with
        /// Liblog (the interface used by the DSLink SDK)
        /// </summary>
        /// <param name="cmdLineOptions"></param>
        private static void InitializeLogging(CommandLineArguments cmdLineOptions)
        {
            if (cmdLineOptions.LogFileFolder != null && !cmdLineOptions.LogFileFolder.EndsWith(Path.DirectorySeparatorChar)) {
                throw new ArgumentException($"Specified LogFileFolder must end with '{Path.DirectorySeparatorChar}'");
            }

            var logConfig = new LoggerConfiguration();
            switch (cmdLineOptions.LogLevel) {
                case LogLevel.Debug:
                logConfig.MinimumLevel.Debug();
                break;

                case LogLevel.Info:
                logConfig.MinimumLevel.Information();
                break;

                case LogLevel.Warning:
                logConfig.MinimumLevel.Warning();
                break;

                case LogLevel.Error:
                logConfig.MinimumLevel.Error();
                break;
            }

            logConfig.WriteTo.Console(outputTemplate: "{Timestamp:MM/dd/yyyy HH:mm:ss} {SourceContext} [{Level}] {Message}{NewLine}{Exception}");
            logConfig.WriteTo.Logger(lc =>
            {
                lc.WriteTo.RollingFile(cmdLineOptions.LogFileFolder + "log-{Date}.txt", retainedFileCountLimit: 3);

            });
            Log.Logger = logConfig.CreateLogger();
        }
    }
}