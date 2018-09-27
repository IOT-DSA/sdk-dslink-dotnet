using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using CommandLine;
using System.IO;

namespace DSLink.Example
{
    public class ExampleDSLink : DSLinkContainer
    {
        private readonly Dictionary<string, Value> _rngValues;
        private readonly Random _random;

        private static void Main(string[] args)
        {
            var results = Parser.Default.ParseArguments<CommandLineArguments>(args)
                 .WithParsed( cmdLineOptions =>
                 {
                     //Init the logging engine
                     InitializeLogging(cmdLineOptions);

                     //Construct a link Configuration
                     var config = new Configuration(cmdLineOptions.LinkName, true, true);
                     
                     //Construct our custom link
                     var dslink = new ExampleDSLink(config, cmdLineOptions);

                     InitializeLink(dslink).Wait();
                 })
                 .WithNotParsed(errors => {
                     Environment.Exit(-1);
                 });

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        
        public static async Task InitializeLink(ExampleDSLink dsLink)
        {
            await dsLink.Connect();
            await dsLink.SaveNodes();
        }

        public ExampleDSLink(Configuration config, CommandLineArguments cmdLineOptions) : base(config)
        {
            //Perform any configuration overrides from command line options
            if (cmdLineOptions.BrokerUrl != null) {
                config.BrokerUrl = cmdLineOptions.BrokerUrl;
            }
            if (cmdLineOptions.Token != null) {
                config.Token = cmdLineOptions.Token;
            }
            if (cmdLineOptions.NodesFileName != null) {
                config.NodesFilename = cmdLineOptions.NodesFileName;
            }
            
            _rngValues = new Dictionary<string, Value>();
            _random = new Random();

            Responder.AddNodeClass("rngAdd", delegate (Node node)
            {
                node.Configs.Set(ConfigType.DisplayName, new Value("Create RNG"));
                node.AddParameter(new Parameter
                {
                    Name = "rngName",
                    ValueType = Nodes.ValueType.String
                });
                node.SetAction(new ActionHandler(Permission.Config, _createRngAction));
            });

            Responder.AddNodeClass("rng", delegate (Node node)
            {
                node.Configs.Set(ConfigType.Writable, new Value(Permission.Read.Permit));
                node.Configs.Set(ConfigType.ValueType, Nodes.ValueType.Number.TypeValue);
                node.Value.Set(0);

                lock (_rngValues)
                {
                    _rngValues.Add(node.Name, node.Value);
                }
            });

            Task.Run(() => _updateRandomNumbers());
        }

        public override void InitializeDefaultNodes()
        {
            Responder.SuperRoot.CreateChild("createRNG", "rngAdd").BuildNode();
        }

        private async void _updateRandomNumbers()
        {
            lock (_rngValues)
            {
                foreach (var kv in _rngValues)
                {
                    kv.Value.Set(_random.Next());
                }
            }
            await Task.Delay(1);
            _updateRandomNumbers();
        }
    
        private async void _createRngAction(InvokeRequest request)
        {
            var rngName = request.Parameters["rngName"].Value<string>();
            if (string.IsNullOrEmpty(rngName)) return;
            if (Responder.SuperRoot.Children.ContainsKey(rngName)) return;

            var newRng = Responder.SuperRoot.CreateChild(rngName, "rng").BuildNode();

            await request.Close();
            await SaveNodes();
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
            logConfig.WriteTo.Logger(lc => {
                lc.WriteTo.RollingFile(cmdLineOptions.LogFileFolder + "log-{Date}.txt", retainedFileCountLimit: 3);

            });
            Log.Logger = logConfig.CreateLogger();
        }
    }
}
