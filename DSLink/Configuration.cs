using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSLink.Connection;
using DSLink.Util;
using DSLink.VFS;

namespace DSLink
{
    public class Configuration
    {
        private IVFS _vfs;

        public readonly string Name;
        public string NodesFilename = "nodes.json";
        private string _keysFolder = "";

        public string KeysFolder
        {
            private get => _keysFolder;
            set
            {
                if (value != null && !value.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    throw new ArgumentException($"Specified KeysFolder must end with '{Path.DirectorySeparatorChar}'");
                }

                _keysFolder = value;
            }
        }

        public readonly bool Requester;
        public readonly bool Responder;
        public bool LoadNodesJson = true;
        public bool DisableMsgpack = false;
        public string Token = "";
        public string BrokerUrl = "http://localhost:8080/conn";
        public string CommunicationFormat = "";
        public uint MaxConnectionCooldown = 60;
        public string StorageFolderPath = ".";
        public Type VFSType = typeof(SystemVFS);

        public string Authentication =>
            UrlBase64.Encode(Sha256.ComputeHash(Encoding.UTF8.GetBytes(RemoteEndpoint.salt).Concat(SharedSecret)
                .ToArray()));

        public string CommunicationFormatUsed =>
            (string.IsNullOrEmpty(CommunicationFormat) ? RemoteEndpoint.format : CommunicationFormat);

        public IEnumerable<byte> SharedSecret => string.IsNullOrEmpty(RemoteEndpoint.tempKey)
            ? new byte[0]
            : KeyPair.GenerateSharedSecret(RemoteEndpoint.tempKey);

        public string DsId => Name + "-" + UrlBase64.Encode(Sha256.ComputeHash(KeyPair.EncodedPublicKey));
        public bool HasToken => !string.IsNullOrEmpty(Token);
        public string TokenParameter => Connection.Token.CreateToken(Token, DsId);

        public IVFS Vfs
        {
            get
            {
                if (VFSType == null)
                {
                    throw new ArgumentException("VFS must not be null");
                }

                return _vfs ?? (_vfs = (IVFS) Activator.CreateInstance(VFSType, StorageFolderPath));
            }
        }

        public RemoteEndpoint RemoteEndpoint { internal set; get; }

        public KeyPair KeyPair { get; private set; }

        public Configuration(string linkName, bool requester = false, bool responder = false)
        {
            Name = linkName;
            Requester = requester;
            Responder = responder;
        }

        internal async Task _initKeyPair()
        {
            var keysFilename = KeysFolder + ".keys";

            KeyPair = new KeyPair();

            if (await Vfs.ExistsAsync(keysFilename))
            {
                using (var stream = new StreamReader(await Vfs.ReadAsync(keysFilename)))
                {
                    var keyContents = stream.ReadLine();
                    KeyPair.LoadFrom(keyContents);
                }
            }
            else
            {
                KeyPair.Generate();

                await Vfs.CreateAsync(keysFilename, false);
                using (var stream = new StreamWriter(await Vfs.WriteAsync(keysFilename)))
                {
                    var keyContents = KeyPair.Save();
                    stream.WriteLine(keyContents);
                }
            }
        }
    }
}