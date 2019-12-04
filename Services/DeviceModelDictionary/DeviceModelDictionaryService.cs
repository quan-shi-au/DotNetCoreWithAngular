using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.DeviceModelDictionary
{
    public class DeviceModelDictionaryService : IDeviceModelDictionaryService
    {

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IRepository<mo.DeviceModel> _deviceModelRepo;

        /// <summary>
        /// Configure application environment variables.
        /// </summary>
        /// <param name="env"></param>
        public DeviceModelDictionaryService(IRepository<mo.DeviceModel> deviceModelRepo)
        {
            _deviceModelRepo = deviceModelRepo;
        }
 

        public IEnumerable<mo.DeviceModel> GetAll()
        {
            try
            {
                var query = from s in _deviceModelRepo.Table
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

        public mo.DeviceModel GetBymanagerId(int wid)
        {
            var query = from s in _deviceModelRepo.Table
                        where s.wId == wid
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }


    }
}
