using System;
using System.IO;
using DSLink.Util;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using PCLStorage;

namespace DSLink.Crypto
{
    /// <summary>
    /// Handles key generation, loading, and saving for handshake.
    /// </summary>
    public class KeyPair
    {
        /// <summary>
        /// Key size.
        /// </summary>
        private const int KEY_SIZE = 256;

        /// <summary>
        /// Curve type, prime256v1.
        /// </summary>
        private const string CURVE = "SECP256R1";

        /// <summary>
        /// Location of key file to load from and save to.
        /// </summary>
        private readonly string _location;

        /// <summary>
        /// BouncyCastle KeyPair.
        /// </summary>
        public AsymmetricCipherKeyPair BcKeyPair;

        /// <summary>
        /// Gets the encoded public key.
        /// </summary>
        public byte[] EncodedPublicKey => ((ECPublicKeyParameters) BcKeyPair.Public).Q.GetEncoded();

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:DSLink.Crypto.KeyPair"/> class.
        /// </summary>
        /// <param name="location">Location </param>
        public KeyPair(string location)
        {
            _location = location;
            BcKeyPair = Load();
        }

        /// <summary>
        /// Generate the KeyPair.
        /// </summary>
        private static AsymmetricCipherKeyPair Generate()
        {
            var generator = new ECKeyPairGenerator();
            var secureRandom = new SecureRandom();
            var keyGenParams = new KeyGenerationParameters(secureRandom, KEY_SIZE);
            generator.Init(keyGenParams);
            return generator.GenerateKeyPair();
        }

        /// <summary>
        /// Load the KeyPair from the file, or generate a new one.
        /// </summary>
        /// <returns>BouncyCastle Asymmetric Cipher KeyPair</returns>
        private AsymmetricCipherKeyPair Load()
        {
            IFileSystem fileSystem = FileSystem.Current;
            IFile file = fileSystem.GetFileFromPathAsync(_location).Result;
            
            if (file != null)
            {
                var reader = new StreamReader(file.OpenAsync(FileAccess.Read).Result);
                string data = reader.ReadLine();

                if (data != null)
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

                    return new AsymmetricCipherKeyPair(pubParams, privParams);
                }
            }
            var key = Generate();
            Save(key);
            return key;
        }

        /// <summary>
        /// Save the specified KeyPair.
        /// </summary>
        /// <param name="keyPair">BouncyCastle Asymmetric KeyPair</param>
        private void Save(AsymmetricCipherKeyPair keyPair)
        {
            byte[] privateBytes = ((ECPrivateKeyParameters) keyPair.Private).D.ToByteArray();
            byte[] publicBytes = ((ECPublicKeyParameters) keyPair.Public).Q.GetEncoded();

            string data = Convert.ToBase64String(privateBytes) + " " + Convert.ToBase64String(publicBytes);

            IFileSystem fileSystem = FileSystem.Current;
            IFolder folder = fileSystem.GetFolderFromPathAsync(".").Result;
            IFile file = folder.CreateFileAsync(_location, CreationCollisionOption.ReplaceExisting).Result;

            using (StreamWriter writer = new StreamWriter(file.OpenAsync(FileAccess.ReadAndWrite).Result))
            {
                writer.WriteLine(data);
            }
        }

        /// <summary>
        /// Generates the shared secret.
        /// </summary>
        /// <returns>Shared secret</returns>
        /// <param name="tempKey">Temporary key from server</param>
        public byte[] GenerateSharedSecret(string tempKey)
        {
            byte[] decoded = UrlBase64.Decode(tempKey);
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
        /// <returns>The parameters.</returns>
        private static ECDomainParameters GetParams()
        {
            var ecp = SecNamedCurves.GetByName(CURVE);
            return new ECDomainParameters(ecp.Curve, ecp.G, ecp.N, ecp.H, ecp.GetSeed());
        }

        /// <summary>
        /// Normalize byte data to 32 bytes.
        /// </summary>
        /// <param name="data">Data.</param>
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
                byte[] normal = new byte[32];
                Array.Copy(data, data.Length - 32, normal, 0, normal.Length);
                data = normal;
            }
            return data;
        }
    }
}
