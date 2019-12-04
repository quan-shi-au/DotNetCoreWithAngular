using mo = ent.manager.Entity.Model.Reporting;
using System.Collections.Generic;

namespace ent.manager.Services.Reporting.Report
{
    public interface ISeatDetailsReportService
    {
 
        bool Add(mo.SeatDetailsReport seatDetailsReport);
        IEnumerable<mo.SeatDetailsReport> GetByReportId(int reportId);
        IEnumerable<mo.SeatDetailsReport> GetByReportIdPaged(int reportId,
            string firstName,
            string lastName,
            string optionalData,
            string deviceName
            );

    }
}
