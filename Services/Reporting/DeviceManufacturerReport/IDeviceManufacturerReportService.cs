using mo = ent.manager.Entity.Model.Reporting;

namespace ent.manager.Services.Reporting.Report
{
    public interface IDeviceManufacturerReportService
    {
 
        bool Add(mo.DeviceManufacturerReport devManufacturerReport);
        mo.DeviceManufacturerReport GetByReportId(int reportId);

    }
}
