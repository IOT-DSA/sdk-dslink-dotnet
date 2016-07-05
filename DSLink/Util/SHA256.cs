using Org.BouncyCastle.Crypto.Digests;

namespace DSLink.Util
{
    internal class SHA256
    {
        public byte[] ComputeHash(byte[] data)
        {
            Sha256Digest sha256 = new Sha256Digest();
            sha256.BlockUpdate(data, 0, data.Length);
            byte[] result = new byte[sha256.GetDigestSize()];
            sha256.DoFinal(result, 0);
            return result;
        }
    }
}
