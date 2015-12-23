using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DSLink.Connection;
using DSLink.Crypto;
using DSLink.Util;

namespace DSLink
{
    public class Configuration
    {
        private readonly SHA256 _sha256;
        public readonly string Name;
        public readonly bool Requester;
        public readonly bool Responder;
        public readonly string BrokerUrl;
        public readonly string KeysLocation;
        private readonly string _communicationFormat;
        private RemoteEndpoint _remoteEndpoint;
        public KeyPair KeyPair { get; }
        public string Authentication => UrlBase64.Encode(_sha256.ComputeHash(Encoding.UTF8.GetBytes(RemoteEndpoint.salt).Concat(SharedSecret).ToArray()));
        public string CommunicationFormat => (string.IsNullOrEmpty(_communicationFormat) ? RemoteEndpoint.format : _communicationFormat);
        public byte[] SharedSecret => string.IsNullOrEmpty(RemoteEndpoint.tempKey) ? new byte[0] : KeyPair.GenerateSharedSecret(RemoteEndpoint.tempKey);
        public string DsId => Name + "-" + UrlBase64.Encode(_sha256.ComputeHash(KeyPair.EncodedPublicKey));

        public RemoteEndpoint RemoteEndpoint
        {
            get
            {
                return _remoteEndpoint;
            }
            set
            {
                if (_remoteEndpoint == null)
                {
                    _remoteEndpoint = value;
                }
            }
        }

        public Configuration(string name, bool requester = false, bool responder = false, string brokerUrl = "http://127.0.0.1:8080/conn", string keysLocation = ".keys", string communicationFormat = "")
        {
            _sha256 = SHA256.Create();
            Name = name;
            Requester = requester;
            Responder = responder;
            BrokerUrl = brokerUrl;
            KeysLocation = keysLocation;
            _communicationFormat = communicationFormat;
            
            KeyPair = new KeyPair(KeysLocation);
        }
    }
}
