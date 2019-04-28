using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSLink.Logging;
using DSLink.Serializer;
using DSLink.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DSLink.Protocol
{
    public class Handshake
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private const string DsaVersion = "1.1.2";
        
        private readonly BaseLinkHandler _link;
        private readonly HttpClientHandler _httpClientHandler;
        private readonly HttpClient _httpClient;

        public Handshake(BaseLinkHandler link)
        {
            _link = link;
            _httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(_httpClientHandler);
        }

        ~Handshake()
        {
            _httpClient.Dispose();
            _httpClientHandler.Dispose();
        }

        private string _buildUrl()
        {
            var url = _link.Config.BrokerUrl;
            url += "?dsId=" + _link.Config.DsId;
            if (_link.Config.HasToken)
            {
                url += "&token=" + _link.Config.TokenParameter;
            }

            return url;
        }

        /// <summary>
        /// Run handshake with the server.
        /// </summary>
        public async Task<RemoteEndpoint> Shake()
        {
            Logger.Info($"Handshaking with {_buildUrl()}");
            HttpResponseMessage resp = null;
            try
            {
                resp = await RunHandshake();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to handshake with broker.");
            }

            if (resp == null || !resp.IsSuccessStatusCode)
            {
                Logger.Error("Failed to handshake with broker.");
                return null;
            }

            Logger.Debug("Handshake successful");
            return JsonConvert.DeserializeObject<RemoteEndpoint>(
                await resp.Content.ReadAsStringAsync()
            );
        }

        /// <summary>
        /// Performs handshake with POST endpoint on the broker.
        /// </summary>
        private Task<HttpResponseMessage> RunHandshake()
        {
            return _httpClient.PostAsync(_buildUrl(),
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
                {"version", DsaVersion},
                {
                    "formats",
                    new JArray(_link.Config.DisableMsgpack
                        ? Serializers.Json.Keys.ToArray()
                        : Serializers.Types.Keys.ToArray())
                },
                {"enableWebSocketCompression", _link.Connection.SupportsCompression}
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