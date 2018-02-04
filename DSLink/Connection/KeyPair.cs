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
        /// BouncyCastle KeyPair.
        /// </summary>
        public AsymmetricCipherKeyPair BcKeyPair;

        /// <summary>
        /// Gets the encoded public key.
        /// </summary>
        public byte[] EncodedPublicKey => ((ECPublicKeyParameters) BcKeyPair.Public).Q.GetEncoded();

        /// <summary>
        /// Generate a new BouncyCastle KeyPair.
        /// </summary>
        public void Generate()
        {
            var generator = new ECKeyPairGenerator();
            var secureRandom = new SecureRandom();
            var keyGenParams = new KeyGenerationParameters(secureRandom, KeySize);
            generator.Init(keyGenParams);
            BcKeyPair = generator.GenerateKeyPair();
        }

        /// <summary>
        /// Used to load keypair from string provided from Save function.
        /// </summary>
        /// <param name="keyContents">Data from Save function</param>
        public void LoadFrom(string keyContents)
        {
            var split = keyContents.Split(' ');
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

        /// <summary>
        /// Generates a string containing the private and public bytes of the
        /// keypair.
        /// </summary>
        public string Save()
        {
            var privateBytes = ((ECPrivateKeyParameters) BcKeyPair.Private).D.ToByteArray();
            var publicBytes = ((ECPublicKeyParameters)BcKeyPair.Public).Q.GetEncoded();
            return Convert.ToBase64String(privateBytes) + " " + Convert.ToBase64String(publicBytes);
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
