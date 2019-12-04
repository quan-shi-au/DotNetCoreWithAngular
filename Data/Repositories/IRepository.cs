

using System.Linq;
using ent.manager.Data.Context;
using ent.manager.Entity.Model;

namespace ent.manager.Data.Repositories
{
    public partial interface IRepository<T> where T :   BaseEntity
    {
        T GetById(object id);
        void Insert(T entity);
        void Update(T entity);
        void Delete(T entity);
        IQueryable<T> Table { get; }
    }
}
