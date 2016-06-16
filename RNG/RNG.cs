using System;
using System.Collections.Generic;
using System.Threading;
using DSLink;
using DSLink.NET;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Util.Logger;
using Action = DSLink.Nodes.Actions.Action;

namespace RNG
{
    class ExampleDSLink : DSLinkContainer
    {
        private Timer timer;
        private int counter;

        public ExampleDSLink(Configuration config) : base(config)
        {
            Thread.Sleep(3000);
            Requester.List("/", (List<Node> obj) =>
            {
                Console.WriteLine("test");
            });

            /*var myNum = Responder.SuperRoot.CreateChild("MyNum")
                .SetDisplayName("My Number")
                .SetType("int")
                .SetValue(0)
                .BuildNode();

            var addNum = Responder.SuperRoot.CreateChild("AddNum")
                .SetDisplayName("Add Number")
                .AddParameter(new Parameter("Number", "int"))
                .SetAction(new Action(Permission.Write, parameters =>
                {
                    myNum.Value.Set(parameters["Number"].Get());
                    return new List<dynamic>();
                }))
                .BuildNode();

                        Responder.SuperRoot.CreateChild("TestAction")
                            .AddParameter(new Parameter("Test", "string"))
                            .AddColumn(new Column("Status", "bool"))
                            .SetAction(new Action(Permission.Write, parameters =>
                            {
                                Console.WriteLine("ran!");
                                Console.WriteLine(parameters.Count);
                                return new List<dynamic>
                                {
                                    true
                                };
                            }));
            */

			var bytes = Responder.SuperRoot.CreateChild("bytes")
				.SetDisplayName("Bytes")
				.SetType("bytes")
				.SetValue(new byte[]{0x01, 0x02, 0x03})
				.SetWritable(Permission.Read)
				.BuildNode();

            /*var testValue = Responder.SuperRoot.CreateChild("testnode")
                .SetConfig("type", new Value("number")).BuildNode();
            testValue.Value.Set(5);

            testValue.Value.Set(1);
            Random random = new Random();
            timer = new Timer(obj =>
            {
                if (testValue.Subscribed)
                {
                    testValue.Value.Set(random.Next(0, 1000));
                }
                Responder.SuperRoot.RemoveChild("Test" + counter);
                Responder.SuperRoot.CreateChild("Test" + ++counter);
            }, null, 1000, 100);*/

        }

        private static void Main(string[] args)
        {
            NETPlatform.Initialize();
            new ExampleDSLink(new Configuration(new string[] { "--log debug" }, "sdk-dotnet", responder: true, requester: true, logLevel: LogLevel.Debug));

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
