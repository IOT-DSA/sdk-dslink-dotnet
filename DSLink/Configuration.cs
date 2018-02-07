using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLink.Connection;
using DSLink.Logger;
using DSLink.Util;
using DSLink.VFS;
using Mono.Options;

namespace DSLink
{
    public class Configuration
    {
        private readonly IEnumerable<string> _args;
        private readonly SHA256 _sha256;
        private IVFS _vfs;

        public readonly string Name;
        public readonly bool Requester;
        public readonly bool Responder;
        public bool LoadNodesJson = true;
        public string Token = "";
        public string BrokerUrl = "http://localhost:8080/conn";
        public string CommunicationFormat = "";
        public LogLevel LogLevel = LogLevel.Info;
        public uint MaxConnectionCooldown = 60;
        public string StorageFolderPath = ".";
        public Type VFSType = typeof(SystemVFS);
        public Type LoggerType = typeof(ConsoleLogger);

        public string Authentication => UrlBase64.Encode(_sha256.ComputeHash(Encoding.UTF8.GetBytes(RemoteEndpoint.salt).Concat(SharedSecret).ToArray()));
        public string CommunicationFormatUsed => (string.IsNullOrEmpty(CommunicationFormat) ? RemoteEndpoint.format : CommunicationFormat);
        public byte[] SharedSecret => string.IsNullOrEmpty(RemoteEndpoint.tempKey) ? new byte[0] : KeyPair.GenerateSharedSecret(RemoteEndpoint.tempKey);
        public string DsId => Name + "-" + UrlBase64.Encode(_sha256.ComputeHash(KeyPair.EncodedPublicKey));
        public bool HasToken => !string.IsNullOrEmpty(Token);
        public string TokenParameter => Connection.Token.CreateToken(Token, DsId);
        public IVFS VFS
        {
            get
            {
                if (VFSType == null)
                {
                    throw new ArgumentException("VFS must not be null");
                }
                if (_vfs == null)
                {
                    _vfs = (IVFS) Activator.CreateInstance(VFSType, StorageFolderPath);
                }
                return _vfs;
            }
        }

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
            _args = args;
            _sha256 = new SHA256();

            Name = name;
            Requester = requester;
            Responder = responder;
        }

        internal async Task _initKeyPair()
        {
            const string KEYS_FILENAME = ".keys";

            KeyPair = new KeyPair();

            if (await VFS.ExistsAsync(KEYS_FILENAME))
            {
                using (var stream = new StreamReader(await VFS.ReadAsync(KEYS_FILENAME)))
                {
                    var keyContents = stream.ReadLine();
                    KeyPair.LoadFrom(keyContents);
                }
            }
            else
            {
                KeyPair.Generate();

                await VFS.CreateAsync(KEYS_FILENAME, false);
                using (var stream = new StreamWriter(await VFS.WriteAsync(KEYS_FILENAME)))
                {
                    var keyContents = KeyPair.Save();
                    stream.WriteLine(keyContents);
                }
            }
        }

        internal void _processOptions()
        {
            var options = new OptionSet
            {
                {
                    "broker=", val => { BrokerUrl = val; }
                },
                {
                    "token=", val => { Token = val; }
                },
                {
                    "log=", val => { LogLevel = LogLevel.ParseLogLevel(val); }
                }
            };
            options.Parse(_args);
        }
    }
}
