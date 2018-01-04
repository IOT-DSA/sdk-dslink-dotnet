using System.Threading;
using DSLink.NET;
using System.Threading.Tasks;

namespace DSLink.Example
{
    public class NETExampleDSLink
    {
        private static void Main(string[] args)
        {
            InitializeLink(args).Wait();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        public static async Task InitializeLink(string[] args)
        {
            NETPlatform.Initialize();

            var config = new Configuration(args, "RNG", true, true)
            {
                BrokerUrl = "http://rnd.iot-dsa.org/conn"
            };
            var dslink = new ExampleDSLink(config);

            await dslink.Connect();
            await dslink.SaveNodes();
        }
    }
}
