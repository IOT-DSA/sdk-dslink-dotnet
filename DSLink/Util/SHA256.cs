using Org.BouncyCastle.Crypto.Digests;

namespace DSLink.Util
{
    internal static class Sha256
    {
        public static byte[] ComputeHash(byte[] data)
        {
            var sha256 = new Sha256Digest();
            sha256.BlockUpdate(data, 0, data.Length);
            var result = new byte[sha256.GetDigestSize()];
            sha256.DoFinal(result, 0);
            return result;
        }
    }
}