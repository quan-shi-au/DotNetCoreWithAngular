using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.SubscriptionAuth
{
    public interface ISubscriptionAuthService
    {
        mo.SubscriptionAuth GetById(int id);
        IEnumerable<mo.SubscriptionAuth> GetAll();
        bool Add(mo.Subscription Subscription);
        mo.SubscriptionAuth GetBySubscriptionId(int subscriptionId);
        string AuthenticateGetLicence(string username, string pin);

    }
}
