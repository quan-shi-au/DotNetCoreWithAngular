using mo = ent.manager.Entity.Model.Reporting;

namespace ent.manager.Services.Reporting.Report
{
    public interface IDeviceTypeReportService
    {
        bool Add(mo.DeviceTypeReport deviceTypeReport);
        mo.DeviceTypeReport GetByReportId(int reportId);
    }
}
