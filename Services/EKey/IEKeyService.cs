using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.Encryption
{
    public interface IEKeyService
    {
        byte[] Encrypt(string plainText, string key, string IV);
        string Decrypt(string encryptedText, string key, string IV);
        bool Save(int enterpriseId);
        mo.EKey GetActive(int enterpriseId);
        List<mo.EKey> GetAll(int enterpriseId);
    }
}
