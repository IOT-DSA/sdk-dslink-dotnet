using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSLink.Connection;
using DSLink.Crypto;
using DSLink.Util;
using DSLink.Util.Logger;
using Mono.Options;

namespace DSLink
{
    /// <summary>
    /// Stores generic information about the DSLink.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// SHA256 cryptography instance
        /// </summary>
        private readonly SHA256 _sha256;

        /// <summary>
        /// Name of the DSLink
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Requester features enabled
        /// </summary>
        public readonly bool Requester;

        /// <summary>
        /// Responder features enabled
        /// </summary>
        public readonly bool Responder;

        /// <summary>
        /// Full URL to Broker
        /// </summary>
        public readonly string BrokerUrl;

        /// <summary>
        /// Location to store generated keypair
        /// </summary>
        public readonly string KeysLocation;

        /// <summary>
        /// Communication format(internal use only)
        /// </summary>
        internal string _communicationFormat;

        /// <summary>
        /// Bouncy Castle KeyPair abstraction
        /// </summary>
        public KeyPair KeyPair { get; }

        /// <summary>
        /// Authentication string
        /// </summary>
        public string Authentication => UrlBase64.Encode(_sha256.ComputeHash(Encoding.UTF8.GetBytes(RemoteEndpoint.salt).Concat(SharedSecret).ToArray()));

        /// <summary>
        /// Communication format
        /// </summary>
        public string CommunicationFormat => (string.IsNullOrEmpty(_communicationFormat) ? RemoteEndpoint.format : _communicationFormat);

        /// <summary>
        /// Shared secret string
        /// </summary>
        public byte[] SharedSecret => string.IsNullOrEmpty(RemoteEndpoint.tempKey) ? new byte[0] : KeyPair.GenerateSharedSecret(RemoteEndpoint.tempKey);

        /// <summary>
        /// DSLink ID
        /// </summary>
        public string DsId => Name + "-" + UrlBase64.Encode(_sha256.ComputeHash(KeyPair.EncodedPublicKey));

        /// <summary>
        /// Remote Endpoint object, stores data that came from the /conn endpoint
        /// </summary>
        internal RemoteEndpoint RemoteEndpoint;

        public LogLevel LogLevel
        {
            private set;
            get;
        }

        /// <summary>
        /// Configuration constructor
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <param name="name">DSLink Name</param>
        /// <param name="requester">Enable requester features</param>
        /// <param name="responder">Enable responder features</param>
        /// <param name="keysLocation">Location to store keys</param>
        /// <param name="communicationFormat">Communication format(json, msgpack)</param>
        /// <param name="brokerUrl">Full URL of broker to connect to</param>
        public Configuration(IEnumerable<string> args, string name, bool requester = false, bool responder = false,
                             string keysLocation = ".keys", string communicationFormat = "",
                             string brokerUrl = "http://localhost:8080/conn", LogLevel logLevel = null)
        {
            if (logLevel == null)
            {
                logLevel = LogLevel.Info;
            }

            _sha256 = new SHA256();

            Name = name;
            Requester = requester;
            Responder = responder;
            KeysLocation = keysLocation;
            _communicationFormat = communicationFormat;

            var options = new OptionSet
            {
                {
                    "broker=", val => { brokerUrl = val; }
                },
                {
                    "log=", val => { logLevel = LogLevel.ParseLogLevel(val); }
                }
            };
            options.Parse(args);

            BrokerUrl = brokerUrl;
            LogLevel = logLevel;

            KeyPair = new KeyPair(KeysLocation);
        }
    }
}
