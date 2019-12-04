using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.Partner
{
    public class PartnerService : IPartnerService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.Partner> _partnerRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public PartnerService(IRepository<mo.Partner> partnerRepo)
        {
            _partnerRepo = partnerRepo;
        }

        public bool Add(mo.Partner partner)
        {
            var result = false;
            try
            {
                if (partner == null)
                    throw new ArgumentNullException("Partner");

                partner.CreationTime = DateTime.UtcNow;

                _partnerRepo.Insert(partner);

                result = true;
            }
       
            catch  
            {
                throw;
            }
           
            return result;
        }

        public bool Delete(mo.Partner partner)
        {
            var result = false;
            try
            {
                if (partner == null)
                    throw new ArgumentNullException("Partner");

                _partnerRepo.Delete(partner);

                result = true;
            }
            catch  
            {
                throw ;
       
            }

            return result;
        }

        public IEnumerable<mo.Partner> GetAll()
        {
            var query = from s in _partnerRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }

        public mo.Partner GetById(int id)
        {
            var query = from s in _partnerRepo.Table
                        where s.Id == id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

        public bool Update(mo.Partner partner)
        {
            var result = false;
            try
            {
                if (partner == null)
                    throw new ArgumentNullException("Partner");

                partner.ModificationTime = DateTime.UtcNow;

                _partnerRepo.Update(partner);

                result = true;
            }
            catch
            {
                throw;

            }

            return result;
        }

        public mo.Partner GetByName(string name)
        {
            var query = from s in _partnerRepo.Table
                        where s.Name == name
                        select s;
            var result = query.FirstOrDefault();
            return result;

        }
    }
}
