using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSLink.Connection.Serializer;
using DSLink.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSLink.Connection
{
    public class Handshake
    {
        /// <summary>
        /// DSA Version.
        /// </summary>
        private const string DSA_VERSION = "1.1.2";

        /// <summary>
        /// Instance of link container.
        /// </summary>
        private readonly DSLinkContainer _link;

        /// <summary>
        /// HttpClient for handshaking.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Connection.Handshake"/> class.
        /// </summary>
        /// <param name="link">Link container</param>
        public Handshake(DSLinkContainer link)
        {
            _link = link;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Run handshake with the server.
        /// </summary>
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

        /// <summary>
        /// Performs handshake with POST endpoint on the broker.
        /// </summary>
        private Task<HttpResponseMessage> RunHandshake()
        {
            return _httpClient.PostAsync(_link.Config.BrokerUrl + "?dsId=" + _link.Config.DsId, 
                new StringContent(GetJson().ToString()));
        }

        /// <summary>
        /// Creates a JSON object with necessary data for handshake.
        /// </summary>
        /// <returns>JObject with necessary data</returns>
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
                {"enableWebSocketCompression", _link.Connector.SupportsCompression}
            };
        }
    }

    /// <summary>
    /// Data received from the handshake's body content.
    /// </summary>
    public class RemoteEndpoint
    {
        /// <summary>
        /// DS Identifier of the broker.
        /// </summary>
        public string dsId;

        /// <summary>
        /// Public key of the server.
        /// </summary>
        public string publicKey;

        /// <summary>
        /// WebSocket URI endpoint.
        /// </summary>
        public string wsUri;

        /// <summary>
        /// HTTP handshake endpoint.
        /// </summary>
        public string httpUri;

        /// <summary>
        /// Temporary key for handshake.
        /// </summary>
        public string tempKey;

        /// <summary>
        /// Salt for the handshake.
        /// </summary>
        public string salt;

        /// <summary>
        /// Path of this link.
        /// </summary>
        public string path;

        /// <summary>
        /// Version of DSA the broker is running.
        /// </summary>
        public string version;

        /// <summary>
        /// Update interval.
        /// </summary>
        public int updateInterval;

        /// <summary>
        /// Serialization format used to communicate with.
        /// </summary>
        public string format = "json";
    }
}
