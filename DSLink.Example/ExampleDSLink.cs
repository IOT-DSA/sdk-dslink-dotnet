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
                     cmdLineOptions = ProcessDSLinkJson(cmdLineOptions);

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

        #region Initialize Logging
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

                case LogLevel.Unspecified:
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
        #endregion

        #region dslink-json file processing
        /// <summary>
        /// This method will return an instance of CommandLineArguments build with the following logic rules.
        /// The file dslink.json can be utilized to specifiy command line arguments.  These live within the config block
        /// of the file.  Here is an example:
        ///         ...
        ///         "configs" : {
        ///                 "broker" : {
        ///                     "type": "url",
        ///                     "value":  "mybroker",
        ///                     "default": "http:localhost:8080\conn"
        ///                 },
        ///              }
        /// 
        /// The code in this method considers only the attribute's name ("broker") and value ("mybroker") in this example).
        /// "type" and "default" are not used.
        /// 
        /// The receives an instance of CommandLineArguments previously built from the parser.  If the dslink-json paramater
        /// is not null the code will use the value specified rather than the default value of "dslink.json" for the file
        /// to read containing the information.
        /// 
        /// Options specified on the command line wins out over those specified in the file.
        /// 
        /// </summary>
        /// <param name="cmdLineOptions"></param>
        /// <returns></returns>
        private static CommandLineArguments ProcessDSLinkJson(CommandLineArguments cmdLineOptions)
        {
            bool errorIfNotFound = false;
            string fileName = "dslink.json";

            //If filename is specified then error if it is not found
            if (!String.IsNullOrEmpty(cmdLineOptions.DSLinkJsonFilename)) {
                errorIfNotFound = true;
                fileName = cmdLineOptions.DSLinkJsonFilename;
            }

            string fileData = "";
            if (File.Exists(fileName)) {
                fileData = File.ReadAllText(fileName);
            }
            else {
                if (errorIfNotFound == true) {
                    throw new ArgumentException($"Specified dslink-json file <{fileName}> was not found");
                }
                else {
                    return cmdLineOptions;
                }
            }

            JObject dslinkJson = JObject.Parse(fileData);
            var dsLinkJsonConfig = dslinkJson["configs"];

            var cmdLineOptionsDslinkJson = new CommandLineArguments();

            cmdLineOptionsDslinkJson.BrokerUrl = GetDsLinkStringValueForAttributeName(dsLinkJsonConfig, "broker", cmdLineOptions.BrokerUrl);
            cmdLineOptionsDslinkJson.LinkName = GetDsLinkStringValueForAttributeName(dsLinkJsonConfig, "name", cmdLineOptions.LinkName);
            cmdLineOptionsDslinkJson.LogFileFolder = GetDsLinkStringValueForAttributeName(dsLinkJsonConfig, "log-file", cmdLineOptions.LogFileFolder);
            cmdLineOptionsDslinkJson.Key = GetDsLinkStringValueForAttributeName(dsLinkJsonConfig, "key", cmdLineOptions.Key);
            cmdLineOptionsDslinkJson.NodesFileName = GetDsLinkStringValueForAttributeName(dsLinkJsonConfig, "nodes", cmdLineOptions.NodesFileName);
            cmdLineOptionsDslinkJson.Token = GetDsLinkStringValueForAttributeName(dsLinkJsonConfig, "token", cmdLineOptions.Token);
            cmdLineOptionsDslinkJson.LogLevel = GetDsLinkLogLevel(dsLinkJsonConfig, cmdLineOptions.LogLevel);

            return cmdLineOptionsDslinkJson;
        }

        private static LogLevel GetDsLinkLogLevel(JToken configObj, LogLevel logLevel)
        {
            if (logLevel != LogLevel.Unspecified) {
                return logLevel;
            }

            string testString = "";
            try {
                testString = configObj["log"]["value"].ToString();
            }
            catch { };

            LogLevel useLogLevel = LogLevel.Info;
            if(!Enum.TryParse(testString, out useLogLevel)) {
                throw new ArgumentException("Invalid 'value' specified for 'log' value in specified dslink-json file.");
            }

            return useLogLevel;
        }

        private static string GetDsLinkStringValueForAttributeName(JToken configObj, string attributeName, string cmdLineValue)
        {
            //use cmdLineValue if specified else attempt to use the one from the dslink-json
            if (cmdLineValue != null) {
                return cmdLineValue;
            }

            try {
                return configObj[attributeName]["value"].ToString();
            }
            catch {
                return null;
            }
        }

        #endregion processing
    }
}
