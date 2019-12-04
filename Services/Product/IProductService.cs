using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.Product
{
    public interface IProductService
    {
        IEnumerable<mo.Product> GetAll();
        mo.Product GetByWId(int id);
    }
}
