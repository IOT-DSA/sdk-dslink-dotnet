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
        public const int AES256KeySize = 256;

        public static byte[] RandomByteArray(int length)
        {
            byte[] result = new byte[length];

            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                provider.GetBytes(result);
                return result;
            }
        }

        public static byte[] AESEncryptBytes(byte[] clear, byte[] password, byte[] salt)
        {

            byte[] encrypted = null;

            var key = GenerateKey(password, salt);

            password = null;
            GC.Collect();

            using (Aes aes = new AesManaged())
            {
                aes.KeySize = AES256KeySize;
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
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
                aes.KeySize = AES256KeySize;
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
            using (Rfc2898DeriveBytes r = GenerateKey(password, salt))
            {
                byte[] newKey = r.GetBytes(AES256KeySize / 8);
                return newKey.SequenceEqual(key);
            }
        }

        public static Rfc2898DeriveBytes GenerateKey(byte[] password, byte[] salt)
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

            IntPtr unmanagedString = IntPtr.Zero;

            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);

            var retBytes = encoding.GetBytes(Marshal.PtrToStringUni(unmanagedString));

            return retBytes;
        }
    }
}
