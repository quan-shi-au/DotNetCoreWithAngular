using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.Subscription
{
    public class SubscriptionService : ISubscriptionService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.Subscription> _subscriptionRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public SubscriptionService(IRepository<mo.Subscription> SubscriptionRepo)
        {
            _subscriptionRepo = SubscriptionRepo;
        }

        public bool Add(mo.Subscription Subscription)
        {
            var result = false;
            try
            {
                if (Subscription == null)
                    throw new ArgumentNullException("Subscription");

                Subscription.CreationTime = DateTime.UtcNow;

                _subscriptionRepo.Insert(Subscription);

                result = true;
            }
            catch { 
            
                throw;
            }
           
            return result;
        }

        public bool Delete(mo.Subscription Subscription)
        {
            var result = false;
            try
            {
                if (Subscription == null)
                    throw new ArgumentNullException("Subscription");

                _subscriptionRepo.Delete(Subscription);

                result = true;
            }
            catch
            {

                throw;  
            }

            return result;
        }

        public IEnumerable<mo.Subscription> GetAll()
        {
            var query = from s in _subscriptionRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }

        public IEnumerable<mo.Subscription> GetActive()
        {
            var query = from s in _subscriptionRepo.Table
                        where s.Status == true
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }

        public mo.Subscription GetById(int id)
        {
            try
            {
                var query = from s in _subscriptionRepo.Table
                            where s.Id == id
                            select s;
                var result = query.FirstOrDefault();
                return result;
            }
            catch  
            {

                throw;
            }
          
        }

        public bool Update(mo.Subscription Subscription)
        {
            var result = false;
         
                if (Subscription == null)
                    throw new ArgumentNullException("Subscription");

                Subscription.ModificationTime = DateTime.UtcNow;

                _subscriptionRepo.Update(Subscription);

                result = true;
         

            return result;
        }

        public mo.Subscription GetByName(string name)
        {
            var query = from s in _subscriptionRepo.Table
                        where s.Name == name
                        select s;
            var result = query.FirstOrDefault();
            return result;

        }

        public IEnumerable<mo.Subscription> GetByEnterpriseId(int eid)
        {
            var query = from s in _subscriptionRepo.Table
                        where s.EnterpriseClientId == eid
                        select s;
            var result = query.ToList();
            return result;

        }

        public mo.Subscription GetByLicenceKey(string licenceKey)
        {
            try
            {
                var query = from s in _subscriptionRepo.Table
                            where s.LicenceKey == licenceKey
                            select s;
                var result = query.FirstOrDefault();
                return result;
            }
            catch
            {

                throw;
            }

        }

    }
}
