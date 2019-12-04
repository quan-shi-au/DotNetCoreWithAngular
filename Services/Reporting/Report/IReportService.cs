using mo = ent.manager.Entity.Model.Reporting;
using System;

namespace ent.manager.Services.Reporting.Report
{
    public interface IReportService
    {
        bool Update(mo.Report report);
        mo.Report Add(mo.Report report);
        mo.Report GetById(int Id);
        mo.Report GetLatestBySubId(int subId);
        mo.Report GetLatestByDateBySubId(int subId, DateTime date);
    }
}
