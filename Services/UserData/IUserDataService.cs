using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.User
{
    public interface IUserDataService
    {
        bool Add(mo.UserData userData);

        List<mo.UserData> GetAll();
        mo.UserData GetById(int id);
 
    }
}
