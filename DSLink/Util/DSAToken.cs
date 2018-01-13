using System;
using System.Text;

namespace DSLink.Util
{
    public static class DSAToken
    {
        private static SHA256 _sha256 = new SHA256();

        public static string CreateToken(string token, string dsId)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Invalid token");
            }
            string tokenHash = dsId + token;
            tokenHash = UrlBase64.Encode(_sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenHash)));
            return token.Substring(0, 16) + tokenHash;
        }
    }
}
