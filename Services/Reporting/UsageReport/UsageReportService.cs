using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model.Reporting;
using System;
using System.Linq;

namespace ent.manager.Services.Reporting.Report
{
    public class UsageReportService : IUsageReportService
    {
        private readonly IRepository<mo.UsageReport> _usageReportRepo;

        public UsageReportService(IRepository<mo.UsageReport> userRepo)
        {
            _usageReportRepo = userRepo;
        }

        public bool Add(mo.UsageReport usageReport)
        {
            var result = false;
            try
            {
                if (usageReport == null)
                    throw new ArgumentNullException("usageReport");

                _usageReportRepo.Insert(usageReport);

                result = true;
            }
            catch
            {

                throw;
            }

            return result;
        }

        public mo.UsageReport GetByReportId(int reportId)
        {
            try
            {
                var query = from s in _usageReportRepo.Table
                            where s.ReportId == reportId
                            select s;
                var result = query.FirstOrDefault();
                return result;
            }
            catch
            {

                throw;
            }

        }


    }
}
