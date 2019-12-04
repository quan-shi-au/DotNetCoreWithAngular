using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.Partner
{
    public interface IPartnerService
    {
        mo.Partner GetById(int id);
        IEnumerable<mo.Partner> GetAll();
        bool Add(mo.Partner employee);
        bool Update(mo.Partner employee);
        bool Delete(mo.Partner employee);
        mo.Partner GetByName(string name);
        

    }
}
