using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSLink;
using DSLink.NET;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Respond;
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
            var testAction = Responder.SuperRoot.CreateChild("test_action")
                                      .SetDisplayName("Test Action")
                                      .AddColumn(new Column("Test", "bool"))
                                      .SetConfig("invokable", new Value("write"))
                                      .SetAction(new Action(Permission.Write, async (Dictionary<string, Value> parameters, DSLink.Request.InvokeRequest request) =>
                                      {
                                          request.SendUpdates(new List<dynamic>
                                          {
                                              true
                                          });
                                          await Task.Delay(1000);
                                          request.SendUpdates(new List<dynamic>
                                          {
                                              false
                                          });
                                          await Task.Delay(1000);
                                          request.Close();
                                      }));

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
                    myNum.Value.Set(myNum.Value.Get() + parameters["Number"].Get());
                    return new List<dynamic>();
                }))
                .BuildNode();

            Responder.SuperRoot.CreateChild("TestAction")
                .AddParameter(new Parameter("Test", "string"))
                .AddColumn(new Column("Status", "bool"))
                     .SetAction(new Action(Permission.Write, (parameters, request) =>
                {
                    Console.WriteLine("ran!");
                    Console.WriteLine(parameters.Count);
                    new List<dynamic>
                    {
                        true
                    };
                }));

            var bytes = Responder.SuperRoot.CreateChild("bytes")
                .SetDisplayName("Bytes")
                .SetType("bytes")
                .SetValue(new byte[] { 0x01, 0x02, 0x03 })
                .SetWritable(Permission.Read)
                .BuildNode();

            var testValue = Responder.SuperRoot.CreateChild("testnode")
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
            }, null, 1000, 10);*/

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
