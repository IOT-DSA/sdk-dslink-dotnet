using System;
using System.IO;
using System.Threading.Tasks;
using DSLink.Util;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using PCLStorage;

namespace DSLink.Connection
{
    /// <summary>
    /// Handles key generation, loading, and saving for handshake.
    /// </summary>
    public class KeyPair
    {
        /// <summary>
        /// Key size.
        /// </summary>
        public const int KeySize = 256;

        /// <summary>
        /// Curve type, prime256v1.
        /// </summary>
        public const string Curve = "SECP256R1";

        /// <summary>
        /// Location of key file to load from and save to.
        /// </summary>
        private readonly string _location;

        /// <summary>
        /// The storage folder for the key.
        /// </summary>
        private readonly IFolder _folder;

        /// <summary>
        /// BouncyCastle KeyPair.
        /// </summary>
        public AsymmetricCipherKeyPair BcKeyPair;

        /// <summary>
        /// Gets the encoded public key.
        /// </summary>
        public byte[] EncodedPublicKey => ((ECPublicKeyParameters) BcKeyPair.Public).Q.GetEncoded();

        public KeyPair(IFolder folder, string location)
        {
            _folder = folder;
            _location = location;
        }

        /// <summary>
        /// Generate the KeyPair.
        /// </summary>
        public static AsymmetricCipherKeyPair Generate()
        {
            var generator = new ECKeyPairGenerator();
            var secureRandom = new SecureRandom();
            var keyGenParams = new KeyGenerationParameters(secureRandom, KeySize);
            generator.Init(keyGenParams);
            return generator.GenerateKeyPair();
        }

        /// <summary>
        /// Load the KeyPair from the file, or generate a new one.
        /// </summary>
        public async Task Load()
        {
            var res = await _folder.CheckExistsAsync(_location);
            switch (res)
            {
                case ExistenceCheckResult.FileExists:
                    await LoadFromFile();
                    break;
                case ExistenceCheckResult.NotFound:
                    await GenerateAndSave();
                    break;
                default:
                    throw new IOException("Unknown error occurred while trying to load/save crypto keypair.");
            }
        }

        private async Task LoadFromFile()
        {
            var file = await _folder.GetFileAsync(_location);
            using (var reader = new StreamReader(await file.OpenAsync(FileAccess.Read)))
            {
                var data = reader.ReadLine();

                if (data != null)
                {
                    DeserializeKeyPair(data);
                }
            }
        }

        private void DeserializeKeyPair(string data)
        {
            var split = data.Split(' ');
            if (split.Length != 2)
            {
                throw new FormatException("Keys file doesn't contain proper data.");
            }

            var ecp = GetParams();

            var q = Convert.FromBase64String(split[1]);
            var point = ecp.Curve.DecodePoint(q);
            var pubParams = new ECPublicKeyParameters(point, ecp);

            var d = new BigInteger(Convert.FromBase64String(split[0]));
            var privParams = new ECPrivateKeyParameters(d, ecp);

            BcKeyPair = new AsymmetricCipherKeyPair(pubParams, privParams);
        }

        private async Task GenerateAndSave()
        {
            var key = Generate();
            await Save(key);
            BcKeyPair = key;
        }

        /// <summary>
        /// Save the specified KeyPair.
        /// </summary>
        /// <param name="keyPair">BouncyCastle Asymmetric KeyPair</param>
        private async Task Save(AsymmetricCipherKeyPair keyPair)
        {
            var privateBytes = ((ECPrivateKeyParameters) keyPair.Private).D.ToByteArray();
            var publicBytes = ((ECPublicKeyParameters) keyPair.Public).Q.GetEncoded();
            var data = Convert.ToBase64String(privateBytes) + " " + Convert.ToBase64String(publicBytes);
            var file = await _folder.CreateFileAsync(_location, CreationCollisionOption.ReplaceExisting);

            using (var writer = new StreamWriter(await file.OpenAsync(FileAccess.ReadAndWrite)))
            {
                writer.WriteLine(data);
            }
        }

        /// <summary>
        /// Generates the shared secret for connecting to the broker.
        /// </summary>
        /// <returns>Shared secret</returns>
        /// <param name="tempKey">Temporary key from server</param>
        public byte[] GenerateSharedSecret(string tempKey)
        {
            var decoded = UrlBase64.Decode(tempKey);
            var privateKey = ((ECPrivateKeyParameters) BcKeyPair.Private);
            var param = privateKey.Parameters;
            var point = param.Curve.DecodePoint(decoded);
            var spec = new ECPublicKeyParameters(point, param);
            point = spec.Q.Multiply(privateKey.D);
            var bi = point.X.ToBigInteger();
            return Normalize(bi.ToByteArray());
        }

        /// <summary>
        /// Get the parameters for the curve.
        /// </summary>
        /// <returns>Parameters for elliptic curve generation</returns>
        private static ECDomainParameters GetParams()
        {
            var ecp = SecNamedCurves.GetByName(Curve);
            return new ECDomainParameters(ecp.Curve, ecp.G, ecp.N, ecp.H, ecp.GetSeed());
        }

        /// <summary>
        /// Normalize byte data to 32 bytes.
        /// </summary>
        /// <param name="data">Data</param>
        private static byte[] Normalize(byte[] data)
        {
            if (data.Length < 32)
            {
                var normal = new byte[32];
                var len = data.Length;
                Array.Copy(data, 0, normal, 32 - len, len);
                data = normal;
            }
            else if (data.Length > 32)
            {
                var normal = new byte[32];
                Array.Copy(data, data.Length - 32, normal, 0, normal.Length);
                data = normal;
            }
            return data;
        }
    }
}
