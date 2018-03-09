using System;
using System.Threading;
using System.Threading.Tasks;
using DSLink.Nodes;
using DSLink.Respond;
using Newtonsoft.Json.Linq;

namespace DSLink.Example.Requester
{
    public class ExampleRequesterDSLink : DSLinkContainer
    {
        public static void Main(string[] args)
        {
            InitializeLink(args).Wait();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        public static async Task InitializeLink(string[] args)
        {
            var config = new Configuration(args, "dotNET-RequesterExample", true)
            {
            };
            var dslink = new ExampleRequesterDSLink(config);

            await dslink.Connect();
        }

        public ExampleRequesterDSLink(Configuration config) : base(config)
        {
        }

        protected override void OnConnectionOpen()
        {
            base.OnConnectionOpen();

            Requester.Invoke("/downstream/DQL/query", Permission.Read, new JObject
            {
                new JProperty("query", "list /downstream/?")
            }, dqlCallback);
        }

        private void dqlCallback(InvokeResponse invokeResponse)
        {
            foreach (var invokeResponseUpdate in invokeResponse.Updates)
            {
                Console.WriteLine(invokeResponseUpdate.ToString());
            }
        }
    }
}