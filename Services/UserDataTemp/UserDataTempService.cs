using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.User
{
    public class UserDataTempService : IUserDataTempService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.UserDataTemp> _userDataRepo;
       


        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public UserDataTempService(IRepository<mo.UserDataTemp> userRepo)
        {
            _userDataRepo = userRepo;
        }

        public bool Add(mo.UserDataTemp userData)
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
 

        public List<mo.UserDataTemp> GetAll()
        {
            var query = from s in _userDataRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result ;
        }

        public mo.UserDataTemp GetById(int id)
        {
            var query = from s in _userDataRepo.Table
                        where s.Id == id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

      
    }
}
