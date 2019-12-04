using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.EnterpriseClient
{
    public class EnterpriseClientService : IEnterpriseClientService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.EnterpriseClient> _enterpriseClientRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public EnterpriseClientService(IRepository<mo.EnterpriseClient> EnterpriseClientRepo)
        {
            _enterpriseClientRepo = EnterpriseClientRepo;
        }

        public bool Add(mo.EnterpriseClient EnterpriseClient)
        {
            var result = false;
            try
            {
                if (EnterpriseClient == null)
                    throw new ArgumentNullException("EnterpriseClient");

                EnterpriseClient.CreationTime = DateTime.UtcNow;

                _enterpriseClientRepo.Insert(EnterpriseClient);

                result = true;
            }
            catch
            {
                throw;
            }

            return result;
        }

        public bool Delete(mo.EnterpriseClient EnterpriseClient)
        {
            var result = false;
            try
            {
                if (EnterpriseClient == null)
                    throw new ArgumentNullException("EnterpriseClient");

                _enterpriseClientRepo.Delete(EnterpriseClient);

                result = true;
            }
            catch
            {
                throw;
            }

            return result;
        }

        public IEnumerable<mo.EnterpriseClient> GetAll()
        {
            try
            {
                var query = from s in _enterpriseClientRepo.Table
                            orderby s.Id
                            select s;
                var result = query.ToList();
                return result;
            }
            catch  
            {

                throw;
            }
         
        }

        public mo.EnterpriseClient GetById(int id)
        {
            var query = from s in _enterpriseClientRepo.Table
                        where s.Id == id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

        public bool Update(mo.EnterpriseClient EnterpriseClient)
        {
            var result = false;
            try
            {
                if (EnterpriseClient == null)
                    throw new ArgumentNullException("EnterpriseClient");

                EnterpriseClient.ModificationTime = DateTime.UtcNow;

                _enterpriseClientRepo.Update(EnterpriseClient);

                result = true;
            }
            catch
            {
                throw;
            }

            return result;
        }

        public mo.EnterpriseClient GetByName(string name)
        {
            var query = from s in _enterpriseClientRepo.Table
                        where s.Name == name
                        select s;
            var result = query.FirstOrDefault();
            return result;

        }

        public IEnumerable<mo.EnterpriseClient> GetByPartnerId(int pid)
        {
            var query = from s in _enterpriseClientRepo.Table
                        where s.PartnerId == pid
                        select s;
            var result = query.ToList();
            return result;

        }


    }
}
