using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.User
{
    public class UserService : IUserService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.User> _userRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public UserService(IRepository<mo.User> userRepo)
        {
            _userRepo = userRepo;
        }

        public bool Add(mo.User User)
        {
            var result = false;
            try
            {
                if (User == null)
                    throw new ArgumentNullException("User");

                User.CreationTime = DateTime.UtcNow;

                _userRepo.Insert(User);

                result = true;
            }
            catch 
            {

                throw;
            }
           
            return result;
        }

        public bool Delete(mo.User User)
        {
            var result = false;
            try
            {
                if (User == null)
                    throw new ArgumentNullException("User");

                _userRepo.Delete(User);

                result = true;
            }
            catch  
            {

                throw;
            }

            return result;
        }

        public IEnumerable<mo.User> GetAll()
        {
            var query = from s in _userRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }

        public IEnumerable<mo.User> GetAllPaged(string fn, string ln, string un, string eid, string pid,string role)
        {
            var query = from s in _userRepo.Table
                        select s;

            if (!string.IsNullOrWhiteSpace(fn))
            {
                query = query.Where(x => x.Firstname != null && x.Firstname.ToLower().Contains(fn.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(ln))
            {
                query = query.Where(x => x.Lastname != null && x.Lastname.ToLower().Contains(ln.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(un))
            {
                query = query.Where(x => x.Username != null && x.Username.ToLower().Contains(un.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(eid))
            {
                query = query.Where(x => x.EnterpriseId != null && x.EnterpriseId == (int.Parse(eid)));
            }

            if (!string.IsNullOrWhiteSpace(pid))
            {
                query = query.Where(x => x.PartnerId != null && x.PartnerId == (int.Parse(pid)));
            }
            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(x => x.Role != null && x.Role.ToLower() == role.ToLower() );
            }

            var result = query.ToList();
            return result;
        }

        public mo.User GetById(int id)
        {
            var query = from s in _userRepo.Table
                        where s.Id == id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

        public mo.User GetByUsername(string username)
        {
            var query = from s in _userRepo.Table
                        where s.Username == username
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

        public IEnumerable<mo.User> GetByEnterpriseId(int id)
        {
            var query = from s in _userRepo.Table
                        where s.EnterpriseId == id
                        select s;
      
            return query;
        }

        public mo.User GetByIdentityUserId(string userId)
        {
            var query = from s in _userRepo.Table
                        where s.UserId == userId
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }
    }
}
