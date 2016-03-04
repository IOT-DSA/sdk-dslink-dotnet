using System;
using System.Collections.Generic;
using System.Threading;
using DSLink;
using DSLink.NET;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using Action = DSLink.Nodes.Actions.Action;

namespace RNG
{
    class ExampleDSLink : DSLinkContainer
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
                .AddParameter(new Parameter("Number", "int"))
                .SetAction(new Action(Permission.Write, parameters =>
                {
                    myNum.Value.Set(parameters["Number"].Get());
                    return new List<dynamic>();
                }))
                .BuildNode();

            /*
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

            /*
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
                        }, null, 1000, 1);
            */
        }

        private static void Main(string[] args)
        {
            NetPlatform.Initialize();
            new ExampleDSLink(new Configuration(args, "sdk-dotnet", responder: true, brokerUrl: "http://octocat.local:8080/conn"));

            Console.ReadLine();
        }
    }
}
