using System.Collections.Generic;
using System.Threading;
using DSLink;
using DSLink.NET;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using System.Threading.Tasks;
using ValueType = DSLink.Nodes.ValueType;
using DSLink.Util.Logger;

namespace RNG
{
    public class ExampleDSLink : DSLinkContainer
    {
        private readonly List<Value> _values = new List<Value>();
        private int _num;

        public ExampleDSLink(Configuration config) : base(config)
        {
            Responder.AddNodeClass("testAction", delegate (Node node)
            {
                node.AddParameter(new Parameter("string", ValueType.String));
                node.AddParameter(new Parameter("int", ValueType.Number));
                node.AddParameter(new Parameter("number", ValueType.Number));
                node.AddColumn(new Column("success", ValueType.Boolean));

                var handler = new ActionHandler(Permission.Write, async (request) =>
                {
                    await request.UpdateTable(new Table
                    {
                        new Row
                        {
                            true
                        }
                    });
                    await request.Close();
                });

                node.SetAction(handler);
            });

            Responder.AddNodeClass("rng", delegate (Node node)
            {
                node.Writable = Permission.Read;
                node.ValueType = ValueType.Number;
                node.Value.Set(0.1);

                lock (_values)
                {
                    _values.Add(node.Value);
                }
            });

            Task.Run(async () =>
            {
                await Task.Delay(5000);
                UpdateRandomNumbers();
            });
        }

        private void UpdateRandomNumbers()
        {
            lock (_values)
            {
                foreach (var value in _values)
                {
                    value.Set(_num++);
                }
            }
            Task.Run(() => UpdateRandomNumbers());
        }

        public override void InitializeDefaultNodes()
        {
            Responder.SuperRoot.CreateChild("Test", "testAction").BuildNode();

            for (var x = 1; x <= 5; x++)
            {
                var node = Responder.SuperRoot.CreateChild($"Container{x}").BuildNode();
                for (var i = 1; i <= 50; i++)
                {
                    node.CreateChild($"TestVal{i}", "rng").BuildNode();
                }
            }
        }

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
            NETPlatform.Initialize();

            var config = new Configuration(args, "RNG", true, true)
            {
                CommunicationFormat = "json",
                //LoadNodesJson = false,
                LogLevel = LogLevel.Debug
            };
            var dslink = new ExampleDSLink(config);

            await dslink.Connect();
            await dslink.SaveNodes();
        }
    }
}
