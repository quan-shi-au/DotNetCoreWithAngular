using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.User
{
    public interface IUserService
    {
        bool Add(mo.User User);
        bool Delete(mo.User User);
        IEnumerable<mo.User> GetAll();
        IEnumerable<mo.User> GetAllPaged(string fn, string ln, string un, string eid, string pid, string role);
        mo.User GetById(int id);
        mo.User GetByUsername(string username);
        mo.User GetByIdentityUserId(string userId);
        IEnumerable<mo.User> GetByEnterpriseId(int id);

    }
}
