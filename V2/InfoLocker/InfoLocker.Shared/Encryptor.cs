using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace InfoLocker
{
    public class Encryptor
    {
        public static string Encrypt(string input, string password)
        {
            if (string.IsNullOrEmpty(input) ||
                string.IsNullOrEmpty(password))
            {
                return input;
            }

            IBuffer unencryptedBuffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            IBuffer iv = null;
            CryptographicKey key = GetEncryptionKey(password, out iv);

            IBuffer encryptedBuffer = CryptographicEngine.Encrypt(key, unencryptedBuffer, iv);

            string output = CryptographicBuffer.EncodeToBase64String(encryptedBuffer);
            return output;
        }

        public static string Decrypt(string input, string password)
        {
            if (string.IsNullOrEmpty(input) ||
                string.IsNullOrEmpty(password))
            {
                return input;
            }

            IBuffer encryptedBuffer = CryptographicBuffer.DecodeFromBase64String(input);
            IBuffer iv = null;
            CryptographicKey key = GetEncryptionKey(password, out iv);

            IBuffer unencryptedBuffer = CryptographicEngine.Decrypt(key, encryptedBuffer, iv);

            string output = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, unencryptedBuffer);
            return output;
        }

        private static CryptographicKey GetEncryptionKey(string password, out IBuffer iv)
        {
            byte[] pwdBytes = Encoding.UTF8.GetBytes(password);
            byte[] keyBytes = new byte[16];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }

            Array.Copy(pwdBytes, keyBytes, len);

            IBuffer keyMaterial = CryptographicBuffer.CreateFromByteArray(keyBytes);
            iv = CryptographicBuffer.CreateFromByteArray(keyBytes);

            SymmetricKeyAlgorithmProvider algorithm = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbcPkcs7);
            CryptographicKey key = algorithm.CreateSymmetricKey(keyMaterial);
            return key;
        }

        /******************** OLD CODE **********************************

        static public void Encrypt(Stream input, Stream output, String password)
        {
            //cryptostream expects data to be a multiple of 128 bits in size
            long padding = input.Length % 16;
            if (padding > 0)
                padding = 16 - padding;

            while (padding > 0)
            {
                input.WriteByte(0);
                padding--;
            }

            input.Seek(0, SeekOrigin.Begin);

            //Uncomment this portion to debug
            //String tempFile = Path.GetTempPath() + "\\temp_save.txt";
            //unencrypted_stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            //while ((bytes_read = decrypted_stream.Read(bytes, 0, length)) > 0)
            //{
            //    unencrypted_stream.Write(bytes, 0, bytes_read);
            //}
            //unencrypted_stream.Close();
            //unencrypted_stream = null;
            //decrypted_stream.Seek(0, SeekOrigin.Begin);

            SymmetricAlgorithm algo = GetAESAlgo(password);
            CryptoStream crypto_stream = new CryptoStream(output, algo.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                int length = 1024;
                int bytes_read = 0;
                Byte[] bytes = new Byte[length];
                while ((bytes_read = input.Read(bytes, 0, length)) > 0)
                    crypto_stream.Write(bytes, 0, bytes_read);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (crypto_stream != null)
                    crypto_stream.Close();
            }
        }

        static public MemoryStream Decrypt(Stream input, String password)
        {
            MemoryStream output = new MemoryStream();

            SymmetricAlgorithm algo = GetAESAlgo(password);
            CryptoStream crypto_stream = new CryptoStream(input, algo.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                int length = 1024;
                int bytes_read = 0;
                Byte[] bytes = new Byte[length];
                while ((bytes_read = crypto_stream.Read(bytes, 0, length)) > 0)
                    output.Write(bytes, 0, bytes_read);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (crypto_stream != null)
                    crypto_stream.Close();
            }

            output.Seek(0, SeekOrigin.Begin);

            //Uncomment this portion to debug
            //String tempFile = Path.GetTempPath() + "\\temp_open.txt";
            //unencrypted_stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write);
            //while ((bytes_read = output.Read(bytes, 0, length)) > 0)
            //{
            //    unencrypted_stream.Write(bytes, 0, bytes_read);
            //}
            //unencrypted_stream.Close();
            //unencrypted_stream = null;
            //output.Seek(0, SeekOrigin.Begin);

            return output;
        }

        static public AesManaged GetAESAlgo(String password)
        {
            AesManaged AES = new AesManaged();
            byte[] pwdBytes = Encoding.UTF8.GetBytes(password);
            byte[] keyBytes = new byte[16];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
                len = keyBytes.Length;
            System.Array.Copy(pwdBytes, keyBytes, len);
            AES.Key = keyBytes;
            AES.IV = keyBytes;
            return AES;
        }

        ***************************************************/
    }
}
