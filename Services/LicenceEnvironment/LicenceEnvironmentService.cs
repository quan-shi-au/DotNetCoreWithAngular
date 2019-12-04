using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.LicenceEnvironment
{
    public class LicenceEnvironmentService : ILicenceEnvironmentService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.LicenceEnvironment> _licenceEnvironmentRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public LicenceEnvironmentService(IRepository<mo.LicenceEnvironment> licenceEnvironmentRepo)
        {
            _licenceEnvironmentRepo = licenceEnvironmentRepo;
        }
 

        public IEnumerable<mo.LicenceEnvironment> GetAll()
        {
            try
            {
                var query = from s in _licenceEnvironmentRepo.Table
                            orderby s.Id
                            select s;
                var result = query.ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
                 
            }
        
        }

        public mo.LicenceEnvironment GetBymanagerId(int wid)
        {
            var query = from s in _licenceEnvironmentRepo.Table
                        where s.wId == wid
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }


    }
}
