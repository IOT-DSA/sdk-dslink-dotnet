using DSLink.Nodes;
using DSLink.Nodes.Actions;
using DSLink.Request;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DSLink.Link1
{
    public class Program 
    {
        private static void Main(string[] args)
        {
            InitializeLink(args).Wait();

            while (true) {
                Thread.Sleep(1000);
            }

        }

        public static async Task InitializeLink(string[] args)
        {
            var config = new Configuration(args, "Link1", true, true)
            {
            };

            var dslink = new Link1(config);

            await dslink.Connect();
        }

    }
}
