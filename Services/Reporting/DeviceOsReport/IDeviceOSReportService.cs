using mo = ent.manager.Entity.Model.Reporting;

namespace ent.manager.Services.Reporting.Report
{
    public interface IDeviceOSReportService
    {
 
        bool Add(mo.DeviceOsReport deviceOsReport);
        mo.DeviceOsReport GetByReportId(int reportId);

    }
}
