using System;
using System.Text;
using DSLink.Util;

namespace DSLink.Connection
{
    public static class Token
    {
        public static string CreateToken(string token, string dsId)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Invalid token");
            }

            var tokenHash = dsId + token;
            tokenHash = UrlBase64.Encode(Sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenHash)));
            return token.Substring(0, 16) + tokenHash;
        }
    }
}