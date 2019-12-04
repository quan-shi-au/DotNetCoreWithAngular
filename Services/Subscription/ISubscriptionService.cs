using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.Subscription
{
    public interface ISubscriptionService
    {
        mo.Subscription GetById(int id);
        IEnumerable<mo.Subscription> GetAll();
        bool Add(mo.Subscription subscription);
        bool Update(mo.Subscription subscription);
        bool Delete(mo.Subscription subscription);
        mo.Subscription GetByName(string name);
        IEnumerable<mo.Subscription> GetByEnterpriseId(int eid);
        IEnumerable<mo.Subscription> GetActive();
        mo.Subscription GetByLicenceKey(string licenceKey);
    }
}
