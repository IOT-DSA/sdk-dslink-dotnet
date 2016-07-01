using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    public class ExampleDSLink : DSLinkContainer
    {
        private Timer timer;
        private int counter;

        public ExampleDSLink(Configuration config) : base(config)
        {
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

            var random = new Random();
            //byte[] buffer;
            //buffer = new byte[4000000];
            //Console.WriteLine("writing");
            //randomBytes.Value.Set(buffer);
            /*var task = new Task(() =>
            {
                while (true)
                {
                    if (testValue.Subscribed)
                    {
                        testValue.Value.Set(random.Next(0, 1000));
                    }
                    Responder.SuperRoot.RemoveChild("Test" + counter);
                    Responder.SuperRoot.CreateChild("Test" + ++counter);
                    Thread.Sleep(1);
                }
            });
            task.Start();*/
            /*timer = new Timer(obj =>
            {
                if (testValue.Subscribed)
                {
                }
            }, null, 1000, 1);*/

        }

        private static void Main(string[] args)
        {
            NETPlatform.Initialize();
            new ExampleDSLink(new Configuration(new List<string>(), "sdk-dotnet", responder: true, requester: true, logLevel: LogLevel.Info));

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
