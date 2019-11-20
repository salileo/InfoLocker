using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace InfoLockerForWP7
{
    public class Encryptor
    {
        public enum Encryption { AES, DES, Rijndael };

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
    }
}
