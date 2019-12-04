using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.DeviceTypeDictionary
{
    public interface IDeviceTypeDictionaryService
    { 
        IEnumerable<mo.DeviceType> GetAll();
        mo.DeviceType GetBymanagerId(int wid);
    }
}

