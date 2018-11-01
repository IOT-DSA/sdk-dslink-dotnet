using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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

        public static bool AESEncryptFile(string filePath, byte[] password, bool delete)
        {

            byte[] salt = RandomByteArray(16);

            using (FileStream fs = new FileStream(filePath + ".enc", FileMode.Create))
            {
                var key = GenerateKey(password, salt);

                password = null;
                GC.Collect();

                using (Aes aes = new AesManaged())
                {
                    aes.KeySize = AES256KeySize;
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Padding = PaddingMode.ISO10126;
                    aes.Mode = CipherMode.CBC;

                    fs.Write(salt, 0, salt.Length);

                    using (CryptoStream cs = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {

                        using (FileStream fsIn = new FileStream(filePath, FileMode.Open))
                        {
                            byte[] buffer = new byte[1];
                            int read;

                            key.Dispose();

                            try
                            {
                                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    cs.Write(buffer, 0, read);
                                }

                                if (delete)
                                {
                                    File.Delete(filePath);
                                }

                                cs.Close();
                                fs.Close();
                                fsIn.Close();

                                return true;
                            }
                            catch (Exception e)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
        }

        public static bool AESDecryptFile(string filePath, byte[] password, bool keep)
        {
            byte[] salt = new byte[16];

            using (FileStream fsIn = new FileStream(filePath, FileMode.Open))
            {
                fsIn.Read(salt, 0, salt.Length);

                var key = GenerateKey(password, salt);

                password = null;
                GC.Collect();

                using (Aes aes = new AesManaged())
                {
                    aes.KeySize = AES256KeySize;
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Padding = PaddingMode.ISO10126;
                    aes.Mode = CipherMode.CBC;

                    using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (FileStream fsOut = new FileStream(filePath.Remove(filePath.Length - 4), FileMode.Create))
                        {
                            byte[] buffer = new byte[1];
                            int read;

                            key.Dispose();

                            try
                            {
                                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fsOut.Write(buffer, 0, buffer.Length);
                                }

                                cs.FlushFinalBlock();

                                fsOut.Close();
                                fsIn.Close();
                                cs.Close();

                                return true;
                            }
                            catch (Exception e)
                            {
                                return false;
                            }
                        }
                    }
                }
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

            //if (unmanagedString != IntPtr.Zero)
            //{
            //    Marshal.ZeroFreeBSTR(unmanagedString);
            //}

            return retBytes;
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
