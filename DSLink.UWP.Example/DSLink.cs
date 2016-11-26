using System.Collections.Generic;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using System.Threading.Tasks;
using ValueType = DSLink.Nodes.ValueType;
using System.Diagnostics;
using DSLink.Util.Logger;

namespace DSLink.UWP.Example
{
    public class ExampleDSLink : DSLinkContainer
    {
        public ExampleDSLink(Configuration config) : base(config)
        {
            Task.Run(async () =>
            {
                await Requester.Subscribe("/data/Test", (update) =>
                {
                    Debug.WriteLine("A: " + update.Value);
                });

                await Task.Delay(5000);

                await Requester.Subscribe("/data/Test", (update) =>
                {
                    Debug.WriteLine("B: " + update.Value);
                });
            });

            Responder.AddNodeClass("testAction", delegate (Node node)
            {
                node.AddParameter(new Parameter("string", "string"));
                node.AddParameter(new Parameter("int", "int"));
                node.AddParameter(new Parameter("number", "number"));
                node.AddColumn(new Column("success", "bool"));

                ActionHandler handler = new ActionHandler(Permission.Write, async (request) =>
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

            List<Value> rngs = new List<Value>();

            Responder.AddNodeClass("rng", delegate (Node node)
            {
                node.Writable = Permission.Read;
                node.ValueType = ValueType.Number;
                node.Value.Set(0.1);
                rngs.Add(node.Value);
            });

            /*Task.Run(async () =>
            {
                await Task.Delay(5000);
                int num = 0;

                while (true)
                {
                    foreach (var rng in rngs)
                    {
                        rng.Set(num++);
                    }
                }
            });*/
        }

        public override void InitializeDefaultNodes()
        {
            Responder.SuperRoot.CreateChild("Test", "testAction").BuildNode();

            for (int i = 1; i <= 50; i++)
            {
                Responder.SuperRoot.CreateChild($"TestVal{i}", "rng").BuildNode();
            }
        }

        public static async Task InitializeLink()
        {
            var dslink =
                new ExampleDSLink(new Configuration(new List<string>(), "sdk-dotnet",
                                                    responder: true, requester: true,
                                                    brokerUrl: "http://192.168.1.90:8090/conn",
                                                    loadNodesJson: false, logLevel: LogLevel.Debug,
                                                    communicationFormat: "json"));

            await dslink.Connect();
        }
    }
}
