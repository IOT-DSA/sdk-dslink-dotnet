using System;
using System.IO;
using DSLink.Util;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace DSLink.Crypto
{
    public class KeyPair
    {
        private const int KEY_SIZE = 256;
        private const string CURVE = "SECP256R1";
        private readonly string _location;
        public AsymmetricCipherKeyPair BcKeyPair;
        public byte[] EncodedPublicKey => ((ECPublicKeyParameters) BcKeyPair.Public).Q.GetEncoded();

        public KeyPair(string location)
        {
            _location = location;
            BcKeyPair = Load();
        }

        private static AsymmetricCipherKeyPair Generate()
        {
            ECKeyPairGenerator generator = new ECKeyPairGenerator();
            SecureRandom secureRandom = new SecureRandom();
            KeyGenerationParameters keyGenParams = new KeyGenerationParameters(secureRandom, KEY_SIZE);
            generator.Init(keyGenParams);
            return generator.GenerateKeyPair();
        }

        private AsymmetricCipherKeyPair Load()
        {
            if (File.Exists(_location))
            {
                using (var reader = new StreamReader(_location))
                {
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
            }
            var key = Generate();
            Save(key);
            return key;
        }

        private void Save(AsymmetricCipherKeyPair keyPair)
        {
            byte[] privateBytes = ((ECPrivateKeyParameters) keyPair.Private).D.ToByteArray();
            byte[] publicBytes = ((ECPublicKeyParameters) keyPair.Public).Q.GetEncoded();

            string data = Convert.ToBase64String(privateBytes) + " " + Convert.ToBase64String(publicBytes);

            using (StreamWriter writer = new StreamWriter(_location))
            {
                writer.WriteLine(data);
            }
        }

        public byte[] GenerateSharedSecret(string tempKey)
        {
            byte[] decoded = UrlBase64.Decode(tempKey);
            var privateKey = ((ECPrivateKeyParameters) BcKeyPair.Private);
            ECDomainParameters param = privateKey.Parameters;
            ECPoint point = param.Curve.DecodePoint(decoded);
            ECPublicKeyParameters spec = new ECPublicKeyParameters(point, param);
            point = spec.Q.Multiply(privateKey.D);
            BigInteger bi = point.X.ToBigInteger();
            return Normalize(bi.ToByteArray());
        }

        private static ECDomainParameters GetParams()
        {
            X9ECParameters ecp = SecNamedCurves.GetByName(CURVE);
            ECCurve curve = ecp.Curve;
            ECPoint g = ecp.G;
            BigInteger n = ecp.N;
            BigInteger h = ecp.H;
            byte[] s = ecp.GetSeed();
            return new ECDomainParameters(curve, g, n, h, s);
        }

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
