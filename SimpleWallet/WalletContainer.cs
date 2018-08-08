using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace SimpleWallet
{
    [Serializable]
    public class WalletContainer
    {
        public List<Wallet> Wallets = new List<Wallet>();

        public static WalletContainer Read(String filePath, String password)
        {
            using (Aes aes = Aes.Create())
            using (SHA512 sha = SHA512.Create())
            {
                aes.KeySize = 256;
                aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(password)).Take(32).ToArray();

                aes.BlockSize = 128;
                aes.IV = sha.ComputeHash(sha.ComputeHash(Encoding.UTF8.GetBytes(password))).Take(16).ToArray();

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                string decryptedData;

                using (FileStream fileStream = File.OpenRead(filePath))
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                {
                    decryptedData = streamReader.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<WalletContainer>(decryptedData);
            }
            
        }

        public void Write(String filePath, String password)
        {
            string serializedObject = JsonConvert.SerializeObject(this);
            using (Aes aes = Aes.Create())
            using (SHA512 sha = SHA512.Create())
            {
                aes.KeySize = 256;
                aes.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(password)).Take(32).ToArray();

                aes.BlockSize = 128;
                aes.IV = sha.ComputeHash(sha.ComputeHash(Encoding.UTF8.GetBytes(password))).Take(16).ToArray();

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (FileStream fileStream = File.OpenWrite(filePath))
                using (CryptoStream cryptoStream = new CryptoStream(fileStream, encryptor, CryptoStreamMode.Write))
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(serializedObject);
                }
            }
        }

        public float GetBalance()
        {
            return Wallets.Sum(x => x.GetBalance());
        }
    }
}
