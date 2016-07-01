using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSLink;
using DSLink.NET;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
using DSLink.Util.Logger;
using Action = DSLink.Nodes.Actions.Action;

namespace RNG
{
    public class ExampleDSLink : DSLinkContainer
    {
        private Random random = new Random();

        public ExampleDSLink(Configuration config) : base(config)
        {
            var testNode = Responder.SuperRoot.CreateChild("test")
                                    .SetDisplayName("Test")
                                    .SetType("bytes")
                                    .SetValue(new byte[] { 0x00, 0x01, 0x02 })
                                    .BuildNode();

            var numberNode = Responder.SuperRoot.CreateChild("number")
                                      .SetDisplayName("Number")
                                      .SetType("number")
                                      .SetValue(0.0)
                                      .BuildNode();

            var numberAction = Responder.SuperRoot.CreateChild("set_number")
                                        .SetDisplayName("Set Number")
                                        .SetWritable(Permission.Write)
                                        .AddParameter(new Parameter("Number", "number"))
                                        .SetAction(new Action(Permission.Write, (Dictionary<string, Value> parameters, InvokeRequest request) =>
                                        {
                                            numberNode.Value.Set(parameters["Number"].Get());
                                            request.Close();
                                        }))
                                        .BuildNode();

            /*var task = new Task(() =>
            {
                //while (true)
                {
                    //byte[] buffer = new byte[random.Next(1, 10)];
                    byte[] buffer = new byte[4000000];
                    random.NextBytes(buffer);
                    testNode.Value.Set(buffer);
                    Thread.Sleep(10);
                }
            });
            task.Start();*/
        }

        protected override void OnConnectionOpen()
        {
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
