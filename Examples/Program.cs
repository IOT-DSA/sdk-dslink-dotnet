using System;
using System.Collections.Generic;
using System.Threading;
using DSLink;
using DSLink.Nodes;
using DSLink.Nodes.Actions;
using Action = DSLink.Nodes.Actions.Action;

namespace Examples
{
    class ExampleDSLink : DSLinkContainer
    {
        private Timer timer;
        private int counter;
        public ExampleDSLink() : base(new Configuration("sdk-dotnet", responder: true, brokerUrl: "http://dglux.rpi.local/conn", communicationFormat: "json"))
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

            /*var testValue = Responder.SuperRoot.CreateChild("testnode")
                .SetConfig("type", new Value("number")).BuildNode();
            testValue.Value.Set(5);

            testValue.Value.Set(1);
            Random random = new Random();
            timer = new Timer(obj =>
            {
                try
                {
                    if (testValue.Subscribed)
                    {
                        testValue.Value.Set(random.Next(0, 1000));
                    }
                    Responder.SuperRoot.CreateChild("Test" + counter++);
                }
                catch
                {
                    Console.WriteLine("caught something.");
                }
            }, null, 2000, 1000);*/
            //var Test = Responder.SuperRoot.CreateChild("testnode").Node;
        }

        private static void Main()
        {
            new ExampleDSLink();

            Console.ReadLine();
        }
    }
}
