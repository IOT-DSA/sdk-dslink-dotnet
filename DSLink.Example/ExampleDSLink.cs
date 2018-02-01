using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
using DSLink.Util.Logger;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DSLink.Example
{
    public class ExampleDSLink : DSLinkContainer
    {
        private readonly Dictionary<string, Value> _rngValues;
        private readonly Random _random;

        private static void Main(string[] args)
        {
            InitializeLink(args).Wait();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        public static async Task InitializeLink(string[] args)
        {
            var config = new Configuration(args, "RNG", true, true)
            {
                LogLevel = LogLevel.Debug
            };
            var dslink = new ExampleDSLink(config);

            await dslink.Connect();
            await dslink.SaveNodes();
        }

        public ExampleDSLink(Configuration config) : base(config)
        {
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
            await Task.Delay(100);
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
    }
}
