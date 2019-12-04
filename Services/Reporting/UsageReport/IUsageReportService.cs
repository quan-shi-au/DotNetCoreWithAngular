using mo = ent.manager.Entity.Model.Reporting;

namespace ent.manager.Services.Reporting.Report
{
    public interface IUsageReportService
    {
        bool Add(mo.UsageReport usageReport);
        mo.UsageReport GetByReportId(int reportId);
    }
}
