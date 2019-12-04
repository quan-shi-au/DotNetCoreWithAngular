using mo = ent.manager.Entity.Model;
using System.Collections.Generic;

namespace ent.manager.Services.LicenceEnvironment
{
    public interface ILicenceEnvironmentService
    {
        
        IEnumerable<mo.LicenceEnvironment> GetAll();
        mo.LicenceEnvironment GetBymanagerId(int wid);



    }
}

