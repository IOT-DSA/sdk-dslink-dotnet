using System.Collections.Generic;
using System.Threading;
using DSLink;
using DSLink.NET;
using DSLink.Util.Logger;
using DSLink.Respond;
using DSLink.Nodes;
using System;

namespace RNG
{
    public class ExampleDSLink : DSLinkContainer
    {
        public ExampleDSLink(Configuration config) : base(config)
        {
            var node = Responder.SuperRoot.CreateChild("Test")
                                   .SetType("number")
                                   .SetValue(0.1)
                                   .BuildNode();

            node.OnSubscribed += (int sid) =>
            {
                Console.WriteLine(string.Format("{0} subscribed", sid));
            };

            node.OnUnsubscribed += (int sid) =>
            {
                Console.WriteLine(string.Format("{0} unsubscribed", sid));
            };
        }

        private static void Main(string[] args)
        {
            Initialize();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        public static async void Initialize()
        {
            NETPlatform.Initialize();
            var dslink =
                new ExampleDSLink(new Configuration(new List<string>(), "sdk-dotnet",
                                                    responder: true, requester: true,
                                                    logLevel: LogLevel.Debug,
                                                    communicationFormat: "json",
                                                    connectionAttemptLimit: -1));

            dslink.Connect();
        }
    }
}
