using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace DSLink.Util
{
    public static class Encryption
    {
        private const int Aes256KeySize = 256;

        public static byte[] RandomByteArray(int length)
        {
            var result = new byte[length];

            using (var provider = new RNGCryptoServiceProvider())
            {
                provider.GetBytes(result);
                return result;
            }
        }

        public static byte[] AESEncryptBytes(byte[] clear, byte[] password, byte[] salt)
        {
            byte[] encrypted;

            var key = GenerateKey(password, salt);

            password = null;
            GC.Collect();

            using (Aes aes = new AesManaged())
            {
                aes.KeySize = Aes256KeySize;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clear, 0, clear.Length);
                        cs.Close();
                    }

                    encrypted = ms.ToArray();
                }

                key.Dispose();
            }

            return encrypted;
        }

        public static byte[] AESDecryptBytes(byte[] encrypted, byte[] password, byte[] salt)
        {
            byte[] decrypted = null;

            var key = GenerateKey(password, salt);

            password = null;
            GC.Collect();

            using (Aes aes = new AesManaged())
            {
                aes.KeySize = Aes256KeySize;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encrypted, 0, encrypted.Length);
                        cs.Close();
                    }

                    decrypted = ms.ToArray();
                }

                key.Dispose();
            }

            return decrypted;
        }

        public static bool CheckPassword(byte[] password, byte[] salt, byte[] key)
        {
            using (var r = GenerateKey(password, salt))
            {
                var newKey = r.GetBytes(Aes256KeySize / 8);
                return newKey.SequenceEqual(key);
            }
        }

        private static Rfc2898DeriveBytes GenerateKey(byte[] password, byte[] salt)
        {
            return new Rfc2898DeriveBytes(password, salt, 52768);
        }

        public static byte[] GetByteArrayFromSecureString(SecureString secureString, Encoding encoding = null)
        {
            if (secureString == null)
            {
                throw new ArgumentNullException(nameof(secureString));
            }

            encoding = encoding ?? Encoding.UTF8;

            var unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            var retBytes = encoding.GetBytes(Marshal.PtrToStringUni(unmanagedString));

            return retBytes;
        }
    }
}