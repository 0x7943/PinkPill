using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PinkPill.Encryption
{
    public class PasswordEncryption : IEncrypter
    {
        
        // TODO Salt
        // TODO randomIV
        public Span<byte> Encrypt(Span<byte> sourceBytes, IEncryptionKey keyParameter)
        {
            var key = keyParameter as StringKey ?? throw new ArgumentException($"{nameof(keyParameter)} is not {nameof(StringKey)}.");
            var passwordBytes = Encoding.UTF8.GetBytes(key.Password);
            
            throw new NotImplementedException();

            //return EncryptAes(sourceBytes, SHA256.HashData(passwordBytes), passwordBytes);
        }

        public Span<byte> Decrypt(Span<byte> encryptedBytes, IEncryptionKey keyParameter)
        {
            var key = keyParameter as StringKey ?? throw new ArgumentException($"{nameof(keyParameter)} is not {nameof(StringKey)}.");
            var passwordBytes = Encoding.UTF8.GetBytes(key.Password);
            throw new NotImplementedException();
            //return (sourceBytes, SHA256.HashData(passwordBytes), passwordBytes);
        }

        static byte[] EncryptAes(byte[] bytes, byte[] Key, byte[] IV)
        {
            // Check arguments.
            
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(bytes);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static byte[] DecryptAes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            byte[] plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream srDecrypt = new MemoryStream())
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            csDecrypt.CopyTo(srDecrypt);
                            csDecrypt.Close();
                            plaintext = srDecrypt.ToArray();
                        }
                    }
                }
            }

            return plaintext;
        }
    }

    public record StringKey(string Password) : IEncryptionKey { }
    
}
