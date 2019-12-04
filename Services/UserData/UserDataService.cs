using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.User
{
    public class UserDataService : IUserDataService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.UserData> _userDataRepo;
       


        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public UserDataService(IRepository<mo.UserData> userRepo)
        {
            _userDataRepo = userRepo;
        }

        public bool Add(mo.UserData userData)
        {
            var result = false;
            try
            {
                if (userData == null)
                    throw new ArgumentNullException("UserData");

                userData.CreationTime = DateTime.UtcNow;

                _userDataRepo.Insert(userData);

                result = true;
            }
            catch 
            {

                throw;
            }
           
            return result;
        }
 

        public List<mo.UserData> GetAll()
        {
            var query = from s in _userDataRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result ;
        }

        public mo.UserData GetById(int id)
        {
            var query = from s in _userDataRepo.Table
                        where s.Id == id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

      
    }
}
