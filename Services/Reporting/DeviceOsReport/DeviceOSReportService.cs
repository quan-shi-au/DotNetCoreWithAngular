using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model.Reporting;
using System;
using System.Linq;

namespace ent.manager.Services.Reporting.Report
{
    public class DeviceOSReportService : IDeviceOSReportService
    {
        private readonly IRepository<mo.DeviceOsReport> _deviceOsReportRepo;

        public DeviceOSReportService(IRepository<mo.DeviceOsReport> deviceOsReportRepo)
        {
            _deviceOsReportRepo = deviceOsReportRepo;
        }

        public bool Add(mo.DeviceOsReport deviceOsReport)
        {
            var result = false;
            try
            {
                if (deviceOsReport == null)
                    throw new ArgumentNullException("deviceOsReport");

                _deviceOsReportRepo.Insert(deviceOsReport);

                result = true;
            }
            catch
            {

                throw;
            }

            return result;
        }

        public mo.DeviceOsReport GetByReportId(int reportId)
        {
            try
            {
                var query = from s in _deviceOsReportRepo.Table
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
