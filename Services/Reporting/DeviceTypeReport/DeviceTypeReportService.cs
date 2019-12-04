using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model.Reporting;
using System;
using System.Linq;

namespace ent.manager.Services.Reporting.Report
{
    public class DeviceTypeReportService : IDeviceTypeReportService
    {
        private readonly IRepository<mo.DeviceTypeReport> _deviceTypeReportRepo;

        public DeviceTypeReportService(IRepository<mo.DeviceTypeReport> deviceTypeReportRepo)
        {
            _deviceTypeReportRepo = deviceTypeReportRepo;
        }

        public bool Add(mo.DeviceTypeReport deviceTypeReport)
        {
            var result = false;
            try
            {
                if (deviceTypeReport == null)
                    throw new ArgumentNullException("deviceTypeReport");

                _deviceTypeReportRepo.Insert(deviceTypeReport);

                result = true;
            }
            catch
            {

                throw;
            }

            return result;
        }

        public mo.DeviceTypeReport GetByReportId(int reportId)
        {
            try
            {
                var query = from s in _deviceTypeReportRepo.Table
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
