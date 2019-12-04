using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model.Reporting;
using System;
using System.Linq;

namespace ent.manager.Services.Reporting.Report
{
    public class DeviceManufacturerReportService : IDeviceManufacturerReportService
    {
        private readonly IRepository<mo.DeviceManufacturerReport> _deviceManufacturerReportRepo;

        public DeviceManufacturerReportService(IRepository<mo.DeviceManufacturerReport> deviceManufacturerReportRepo)
        {
            _deviceManufacturerReportRepo = deviceManufacturerReportRepo;
        }

        public bool Add(mo.DeviceManufacturerReport deviceManufacturerReport)
        {
            var result = false;
            try
            {
                if (deviceManufacturerReport == null)
                    throw new ArgumentNullException("deviceManufacturerReport");

                _deviceManufacturerReportRepo.Insert(deviceManufacturerReport);

                result = true;
            }
            catch
            {

                throw;
            }

            return result;
        }


        public mo.DeviceManufacturerReport GetByReportId(int reportId)
        {
            try
            {
                var query = from s in _deviceManufacturerReportRepo.Table
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
