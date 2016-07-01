using System;
using System.Collections.Generic;
using System.Threading;
using DSLink;
using DSLink.NET;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
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
                                      .SetAction(new Action(Permission.Write, (Dictionary<string, Value> parameters, InvokeRequest request) =>
                                      {
                                          request.SendUpdates(new List<dynamic>
                                          {
                                              new List<dynamic>
                                              {
                                                  true
                                              }
                                          });
                                          request.Close();
                                      }));

            var myNum = Responder.SuperRoot.CreateChild("MyNum")
                .SetDisplayName("My Number")
                .SetType("int")
                .SetValue(0)
                .BuildNode();

            var addNum = Responder.SuperRoot.CreateChild("AddNum")
                .SetDisplayName("Add Number")
                .AddParameter(new Parameter("Number", "number"))
                .SetAction(new Action(Permission.Write, (parameters, request) =>
                {
                    myNum.Value.Set(myNum.Value.Get() + parameters["Number"].Get());
                    request.Close();
                }))
                .BuildNode();

            var randomBytes = Responder.SuperRoot.CreateChild("Bytes")
                                       .SetDisplayName("bytes")
                                       .SetType("bytes")
                                       .SetValue(new byte[] { 0x00 })
                                       .BuildNode();

            //var random = new Random();
            //byte[] buffer;
            //buffer = new byte[4000000];
            //Console.WriteLine("writing");
            //randomBytes.Value.Set(buffer);


            /*Responder.SuperRoot.CreateChild("TestAction")
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
            new ExampleDSLink(new Configuration(new List<string>(), "sdk-dotnet", responder: true, requester: true, logLevel: LogLevel.Debug));

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
