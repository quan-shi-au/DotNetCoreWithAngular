using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;
using ent.manager.Entity.Model;
using System.Security.Cryptography;
using System.IO;

namespace ent.manager.Services.Encryption
{
    public class EKeyService : IEKeyService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.EKey> _ekeyRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public EKeyService(IRepository<mo.EKey> partnerRepo)
        {
            _ekeyRepo = partnerRepo;
        }

        public bool Save(int enterpriseId)
        {
            var result = false;
            try
            {
                EKey ekey = new EKey();

                ekey.Active = true;
                ekey.CreationTime = DateTime.UtcNow;

                ekey.EnterpriseId = enterpriseId;
                var generateKeyResult = generateKey();

                if (string.IsNullOrEmpty(generateKeyResult.Key) || string.IsNullOrEmpty(generateKeyResult.IV))
                    return false;

                ekey.Key = generateKeyResult.Key;
                ekey.IV = generateKeyResult.IV;

                _ekeyRepo.Insert(ekey);

                result = true;
            }

            catch(Exception ex)
            {
                throw;
            }

            return result;
        }

        public mo.EKey GetActive(int enterpriseId)
        {
            var query = from s in _ekeyRepo.Table
                        where s.EnterpriseId == enterpriseId && s.Active == true

                        orderby s.Id
                        select s;
            var result = query.FirstOrDefault(); ;
            return result;
        }

        public List<mo.EKey> GetAll(int enterpriseId)
        {
            var query = from s in _ekeyRepo.Table
                        where s.EnterpriseId == enterpriseId
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }

        public byte[] Encrypt(string plainText, string key, string IV)
        {
            try
            {
                var keyArray = Convert.FromBase64String(key);

                var IVArray = Convert.FromBase64String(IV);

                var byteResult = EncryptStringToBytes_Aes(plainText, keyArray, IVArray);

            

                return byteResult;
            }
            catch (Exception)
            {

                throw;
            }

        }

        public string Decrypt(string encryptedText, string key, string IV)
        {
            try
            {
                var keyArray = Convert.FromBase64String(key);

                var IVArray = Convert.FromBase64String(IV);

                var plainTextArray = Convert.FromBase64String(encryptedText);

                var decryptedText = DecryptStringFromBytes_Aes(plainTextArray, keyArray, IVArray);

                return decryptedText;
            }
            catch (Exception)
            {

                throw;
            }

        }

        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
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

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
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
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        private struct KeysStriuct
        {
            public string Key { get; set; }
            public string IV { get; set; }
        }

        private KeysStriuct generateKey()
        {
            KeysStriuct result = new KeysStriuct();
            try
            {

                Aes myAes = Aes.Create();

                result.Key = Convert.ToBase64String(myAes.Key) ;
                result.IV  = Convert.ToBase64String(myAes.IV);

                return result;
            }
            catch (Exception)
            {

                return new KeysStriuct();
            }

        }
    }
}
