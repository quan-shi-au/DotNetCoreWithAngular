using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model.Reporting;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ent.manager.Services.Reporting.Report
{
    public class SeatDetailsReportService : ISeatDetailsReportService
    {
        private readonly IRepository<mo.SeatDetailsReport> _seatDetailsReportRepo;

        public SeatDetailsReportService(IRepository<mo.SeatDetailsReport> seatDetailsReportRepo)
        {
            _seatDetailsReportRepo = seatDetailsReportRepo;
        }

        public bool Add(mo.SeatDetailsReport seatDetailsReport)
        {
            var result = false;
            try
            {
                if (seatDetailsReport == null)
                    throw new ArgumentNullException("seatDetailsReport");

                _seatDetailsReportRepo.Insert(seatDetailsReport);

                result = true;
            }
            catch
            {

                throw;
            }

            return result;
        }

        public IEnumerable<mo.SeatDetailsReport> GetByReportId(int reportId)
        {
            try
            {
                var query = from s in _seatDetailsReportRepo.Table
                            where s.ReportId == reportId
                            select s;
                var result = query;
                return result;
            }
            catch
            {

                throw;
            }

        }


        public IEnumerable<mo.SeatDetailsReport> GetByReportIdPaged(int reportId,
            string firstName,
            string lastName,
            string optionalData, 
            string deviceName
            )
        {
            try
            {
                var query = from s in _seatDetailsReportRepo.Table
                            where s.ReportId == reportId
                            select s;

                if(!string.IsNullOrEmpty(firstName))
                {
                    query = query.Where(x => x.FirstName != null && x.FirstName.ToLower().Contains(firstName.ToLower()));
                }

                if (!string.IsNullOrEmpty(lastName))
                {
                    query = query.Where(x => x.LastName != null && x.LastName.ToLower().Contains(lastName.ToLower()));
                }

                if (!string.IsNullOrEmpty(optionalData))
                {
                    query = query.Where(x => x.OptionalData != null && x.OptionalData.ToLower().Contains(optionalData.ToLower()));
                }

                if (!string.IsNullOrEmpty(deviceName))
                {
                    query = query.Where(x => x.DeviceName != null && x.DeviceName.ToLower().Contains(deviceName.ToLower()));
                }

                var result = query;
                return result;
            }
            catch
            {

                throw;
            }

        }


    }
}
