using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.EnterpriseClient
{
    public interface IEnterpriseClientService
    {
        mo.EnterpriseClient GetById(int id);
        IEnumerable<mo.EnterpriseClient> GetAll();
        bool Add(mo.EnterpriseClient EnterpriseClient);
        bool Update(mo.EnterpriseClient EnterpriseClient);
        bool Delete(mo.EnterpriseClient EnterpriseClient);
        mo.EnterpriseClient GetByName(string name);
        IEnumerable<mo.EnterpriseClient> GetByPartnerId(int pid);

    }
}
