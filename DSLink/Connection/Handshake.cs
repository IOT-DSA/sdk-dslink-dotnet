using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSLink.Connection.Serializer;
using DSLink.Util;
using System.Reflection;
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
                _link.Logger.Info("Handshaking with " + _link.Config.BrokerUrl + "?dsId=" + _link.Config.DsId);
                HttpResponseMessage resp = null;
                try
                {
                    resp = RunHandshake().Result;
                }
                catch (AggregateException e)
                {
                    foreach (var innerException in e.InnerExceptions)
                    {
                        Debug.WriteLine(innerException.Message);
                    }
                }
                    

                if (resp != null && resp.StatusCode == HttpStatusCode.OK)
                {
                    _link.Logger.Info("Handshake successful");
                    _link.Config.RemoteEndpoint = JsonConvert.DeserializeObject<RemoteEndpoint>(resp.Content.ReadAsStringAsync().Result);
                    break;
                }

                Task.Delay(delay * 1000).Wait();

                if (delay <= 60)
                {
                    delay++;
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
                {"formats", new JArray(SerializationManager.Serializers.Keys.ToArray())},
                {"enableWebSocketCompression", _link.Connector.SupportsCompression()}
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
