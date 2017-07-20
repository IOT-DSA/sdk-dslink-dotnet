using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLink.Connection;
using DSLink.Platform;
using DSLink.Util;
using DSLink.Util.Logger;
using Mono.Options;

namespace DSLink
{
    public class Configuration
    {
        private readonly IEnumerable<string> _args;
        private readonly SHA256 _sha256;
        public readonly string Name;
        public readonly bool Requester;
        public readonly bool Responder;
        public bool LoadNodesJson = true;
        public string BrokerUrl = "http://localhost:8080/conn";
        public string KeysLocation = ".keys";
        public string CommunicationFormat = "";
        public LogLevel LogLevel = LogLevel.Info;
        public uint MaxConnectionCooldown = 60;

        public string Authentication => UrlBase64.Encode(_sha256.ComputeHash(Encoding.UTF8.GetBytes(RemoteEndpoint.salt).Concat(SharedSecret).ToArray()));
        public string CommunicationFormatUsed => (string.IsNullOrEmpty(CommunicationFormat) ? RemoteEndpoint.format : CommunicationFormat);
        public byte[] SharedSecret => string.IsNullOrEmpty(RemoteEndpoint.tempKey) ? new byte[0] : KeyPair.GenerateSharedSecret(RemoteEndpoint.tempKey);
        public string DsId => Name + "-" + UrlBase64.Encode(_sha256.ComputeHash(KeyPair.EncodedPublicKey));

        public RemoteEndpoint RemoteEndpoint
        {
            internal set;
            get;
        }

        public KeyPair KeyPair
        {
            get;
            internal set;
        }

        public Configuration(IEnumerable<string> args, string name, bool requester = false, bool responder = false)
        {
            if (BasePlatform.Current == null)
            {
                throw new Exception("Platform specific code was not initialized.");
            }

            _args = args;
            _sha256 = new SHA256();

            Name = name;
            Requester = requester;
            Responder = responder;

            if (!string.IsNullOrEmpty(BasePlatform.Current.GetCommunicationFormat()))
                CommunicationFormat = BasePlatform.Current.GetCommunicationFormat();
        }

        internal async Task _initKeyPair()
        {
            var storage = await BasePlatform.Current.GetStorageFolder();
            KeyPair = new KeyPair(storage, KeysLocation);
            await KeyPair.Load();
        }

        internal void _processOptions()
        {
            var options = new OptionSet
            {
                {
                    "broker=", val => { BrokerUrl = val; }
                },
                {
                    "log=", val => { LogLevel = LogLevel.ParseLogLevel(val); }
                }
            };
            options.Parse(_args);
        }
    }
}
