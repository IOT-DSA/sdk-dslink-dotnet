using System.Linq;
using System.Net;
using System.Threading;
using DSLink.Connection.Serializer;
using DSLink.Util;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSLink.Connection
{
    public class Handshake
    {
        private const string DSA_VERSION = "1.1.2";

        private readonly DSLinkContainer _link;
        private readonly HttpClient _httpClient;

        public Handshake(DSLinkContainer link)
        {
            _link = link;
            _httpClient = new HttpClient();
        }

        public void Shake()
        {
            var keepTrying = true;
            var delay = 1;
            while (keepTrying)
            {
                _link.Logger.Info("Connecting to " + _link.Config.BrokerUrl);
                var resp = RunHandshake().Result;

                if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                {
                    _link.Logger.Info("Connected");
                    _link.Config.RemoteEndpoint = JsonConvert.DeserializeObject<RemoteEndpoint>(resp.Content.ReadAsStringAsync().Result);
                    break;
                }

                try
                {
                    Thread.Sleep(delay * 1000);

                    if (delay <= 60)
                    {
                        delay++;
                    }
                }
                catch (ThreadInterruptedException)
                {
                    keepTrying = false;
                }
            }
        }

        private Task<HttpResponseMessage> RunHandshake()
        {
            return _httpClient.PostAsync(_link.Config.BrokerUrl + "?dsId=" + _link.Config.DsId, 
                new StringContent(GetJson().ToString()));
        }

        private JObject GetJson()
        {
            return new JObject
            {
                {"publicKey", UrlBase64.Encode(_link.Config.KeyPair.EncodedPublicKey)},
                {"isRequester", _link.Config.Requester},
                {"isResponder", _link.Config.Responder},
                {"linkData", new JObject()},
                {"version", DSA_VERSION},
                {"formats", new JArray(SerializationManager.Serializers.Keys.ToArray())}
            };
        }
    }

    public class RemoteEndpoint
    {
        // ReSharper disable InconsistentNaming
        public string dsId;
        public string publicKey;
        public string wsUri;
        public string httpUri;
        public string tempKey;
        public string salt;
        public string path;
        public string version;
        public int updateInterval;
        public string format = "json";
    }
}
