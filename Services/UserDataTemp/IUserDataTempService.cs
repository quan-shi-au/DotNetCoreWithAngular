using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.User
{
    public interface IUserDataTempService
    {
        bool Add(mo.UserDataTemp userData);

        List<mo.UserDataTemp> GetAll();
        mo.UserDataTemp GetById(int id);
 
    }
}
