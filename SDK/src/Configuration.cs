using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DSLink.Connection;
using DSLink.Crypto;
using DSLink.Util;
using Mono.Options;

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

        public Configuration(string name, bool requester = false, bool responder = false, string keysLocation = ".keys", string communicationFormat = "")
        {
            _sha256 = SHA256.Create();
            Name = name;
            Requester = requester;
            Responder = responder;
            KeysLocation = keysLocation;
            _communicationFormat = communicationFormat;

            var brokerUrl = "http://localhost:8080/conn";
            var options = new OptionSet
            {
                {
                    "broker=", val => { brokerUrl = val; }
                }
            };
            options.Parse(Environment.GetCommandLineArgs());

            BrokerUrl = brokerUrl;

            KeyPair = new KeyPair(KeysLocation);
        }
    }
}
