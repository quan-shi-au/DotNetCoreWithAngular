using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System.Linq;

namespace ent.manager.Services.Product
{
    public class ProductService : IProductService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.Product> _productRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public ProductService(IRepository<mo.Product> productRepo)
        {
            _productRepo = productRepo;
        }
 

        public IEnumerable<mo.Product> GetAll()
        {
            var query = from s in _productRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }

        public mo.Product GetByWId(int id)
        {
            var query = from s in _productRepo.Table
                        where s.wId == id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }


    }
}
