using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.DeviceTypeDictionary
{
    public class DeviceTypeDictionaryService : IDeviceTypeDictionaryService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.DeviceType> _deviceTypeRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public DeviceTypeDictionaryService(IRepository<mo.DeviceType> deviceTypeRepo)
        {
            _deviceTypeRepo = deviceTypeRepo;
        }
 

        public IEnumerable<mo.DeviceType> GetAll()
        {
            try
            {
                var query = from s in _deviceTypeRepo.Table
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

        public mo.DeviceType GetBymanagerId(int wid)
        {
            var query = from s in _deviceTypeRepo.Table
                        where s.wId == wid
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }


    }
}
