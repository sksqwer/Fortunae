using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;


namespace GB
{
    public static class AESCrypto
    {
        public static string GenerateKeyOrIV(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }
        
        public static string Encrypt(string key, string textToEncrypt)
        {
            RijndaelManaged rijndaelCipher = GetRijndaelCipher(key);
            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
            return Convert.ToBase64String(rijndaelCipher.CreateEncryptor().TransformFinalBlock(plainText, 0, plainText.Length));
        }
        public static string Decrypt(string key, string textToDecrypt)
        {
            RijndaelManaged rijndaelCipher = GetRijndaelCipher(key);
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plainText);
        }

        static RijndaelManaged GetRijndaelCipher(string key)
        {
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[16];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length) len = keyBytes.Length;
            Array.Copy(pwdBytes, keyBytes, len);

            return new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                BlockSize = 128,
                Key = keyBytes,
                IV = keyBytes
            };
        }
    }
}