using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;
using ent.manager.Services.Subscription;
using ent.manager.Services.EnterpriseClient;

namespace ent.manager.Services.SubscriptionAuth
{
    public class SubscriptionAuthService : ISubscriptionAuthService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.SubscriptionAuth> _subscriptionAuthRepo;
        private readonly ISubscriptionService _subscriptionService;
 
        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public SubscriptionAuthService(
            IRepository<mo.SubscriptionAuth> SubscriptionAuthRepo,
            ISubscriptionService subscriptionService)

        {
            _subscriptionAuthRepo = SubscriptionAuthRepo;
            _subscriptionService = subscriptionService;
  
        }

        public mo.SubscriptionAuth GetById(int id)
        {
            try
            {
                var query = from s in _subscriptionAuthRepo.Table
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

        public IEnumerable<mo.SubscriptionAuth> GetAll()
        {
            var query = from s in _subscriptionAuthRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }


        public bool Add(mo.Subscription Subscription)
        {
            var result = false;
            try
            {
                if (Subscription == null)
                    throw new ArgumentNullException("Subscription");


                var name = Subscription.Name.Substring(0, 6);


                var query = from s in _subscriptionAuthRepo.Table
                            where s.Username.Substring(0,6) == name
                            orderby s.Id
                            select s;
                var existingName = query.ToList();

                //prepare object
                var SubscriptionAuth = new mo.SubscriptionAuth()
                {
                    SubscriptionId = Subscription.Id,
                     Pin =string.Empty,
                     Username = string.Empty
                };

                var currenSequence = existingName.Count() + 1;
                SubscriptionAuth.Username = name + (currenSequence > 9 ? currenSequence.ToString() : "0" + currenSequence.ToString());

                SubscriptionAuth.Pin = GeneratePin();
                _subscriptionAuthRepo.Insert(SubscriptionAuth);

                result = true;
            }
            catch(Exception ex)
            {

                throw;
            }

            return result;
        }

       

        public  mo.SubscriptionAuth GetBySubscriptionId(int subscriptionId)
        {
            try
            {
                var query = from s in _subscriptionAuthRepo.Table
                            where s.SubscriptionId == subscriptionId
                            select s;
                var result = query.ToList();
                return result.FirstOrDefault();
            }
            catch  
            {

                throw;
            }
           

        }

       public string AuthenticateGetLicence(string username, string pin)
        {
            try
            {
                var query = from s in _subscriptionAuthRepo.Table
                            where s.Username.ToLower() == username.ToLower()
                            && s.Pin == pin
                            select s;
                var subAuth = query.FirstOrDefault();
                if (subAuth == null) return string.Empty;


                var subscription = _subscriptionService.GetById(subAuth.SubscriptionId);
                if (subscription == null) return string.Empty;

                return subscription.LicenceKey;
            }
            catch 
            {

                throw;
            }
           

        }


        private string GeneratePin()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }

    }
}
