using System;

namespace DSLink.Util
{
    public class UrlBase64
    {
        public static string Encode(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static byte[] Decode(string data)
        {
            return Convert.FromBase64String(data.Replace('-', '+').Replace('_', '/').PadRight(data.Length + (4 - data.Length % 4) % 4, '='));
        }
    }
}
