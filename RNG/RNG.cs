using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DSLink;
using DSLink.NET;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using System.Threading.Tasks;

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
                node.AddParameter(new Parameter("string", "string"));
                node.AddParameter(new Parameter("int", "int"));
                node.AddParameter(new Parameter("number", "number"));
                node.AddColumn(new Column("success", "bool"));

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
                _values.Add(node.Value);
            });

            Task.Run(async () =>
            {
                await Task.Delay(5000);
                UpdateRandomNumbers();
            });
        }

        private void UpdateRandomNumbers()
        {
            foreach (var value in _values)
            {
                value.Set(_num++);
            }
            Task.Run(() => UpdateRandomNumbers());
        }

        public override void InitializeDefaultNodes()
        {
            Responder.SuperRoot.CreateChild("Test", "testAction").BuildNode();

            for (int i = 1; i <= 50; i++)
            {
                Responder.SuperRoot.CreateChild($"TestVal{i}", "rng").BuildNode();
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
            var dslink =
                new ExampleDSLink(new Configuration(args.ToList(), "sdk-dotnet-rng",
                                                    responder: true, requester: true,
                                                    communicationFormat: "msgpack"));

            await dslink.Connect();
        }
    }
}
