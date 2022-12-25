using System.Security.Cryptography;
using System.Text;

namespace Report_App_WASM.Server.Utils.EncryptDecrypt
{
    public static class EncryptDecrypt
    {
        private static string Secretkey()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return HashKey.Key;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static string? EncryptString(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                var empty = "";
                return empty;
            }
            //only for PKCS7 and AES. Don't change the configuration without change this check
            if (text.Length == 44 && text.EndsWith("="))
            {
                return text;
            }
            var getKey = Secretkey();
            var key = Encoding.UTF8.GetBytes(getKey);

            using var aesAlg = Aes.Create();
            using var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV);
            aesAlg.Padding = PaddingMode.PKCS7;
            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(text);
            }

            var iv = aesAlg.IV;

            var decryptedContent = msEncrypt.ToArray();

            var result = new byte[iv.Length + decryptedContent.Length];

            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

            return Convert.ToBase64String(result);
        }

        public static string? DecryptString(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                var empty = "";
                return empty;
            }
            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                var iv = new byte[16];
                var cipher = new byte[16];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
                var getKey = Secretkey();
                var key = Encoding.UTF8.GetBytes(getKey);

                using var aesAlg = Aes.Create();
                aesAlg.Padding = PaddingMode.PKCS7;
                using var decryptor = aesAlg.CreateDecryptor(key, iv);
                string? result;
                using (var msDecrypt = new MemoryStream(cipher))
                {
                    using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                    using var srDecrypt = new StreamReader(csDecrypt);
                    result = srDecrypt.ReadToEnd();
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has been raised during conversion: {ex}");
                return cipherText;
            }
        }

    }
}
