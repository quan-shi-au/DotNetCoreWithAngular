using System.Collections.Generic;
using mo = ent.manager.Entity.Model.Reporting;

namespace ent.manager.Services.Reporting.ReportProcessorRun
{
    public interface IReportProcessorRunService
    {
        bool Update(mo.ReportProcessorRun reportProcessorRun);
        bool Add(mo.ReportProcessorRun reportProcessorRun);
        mo.ReportProcessorRun GetLatestRun();
        mo.ReportProcessorRun GetById(int Id);
        IEnumerable<mo.ReportProcessorRun> GetAll();



    }
}
