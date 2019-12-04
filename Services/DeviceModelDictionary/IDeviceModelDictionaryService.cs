using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.DeviceModelDictionary
{
    public interface IDeviceModelDictionaryService
    {
        
        IEnumerable<mo.DeviceModel> GetAll();
        mo.DeviceModel GetBymanagerId(int wid);

    }
}

