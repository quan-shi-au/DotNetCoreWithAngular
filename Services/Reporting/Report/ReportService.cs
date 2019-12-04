using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model.Reporting;
using System;
using System.Linq;

namespace ent.manager.Services.Reporting.Report
{
    public class ReportService : IReportService
    {
        private readonly IRepository<mo.Report> _reportRepo;

        public ReportService(IRepository<mo.Report> userRepo)
        {
            _reportRepo = userRepo;
        }

        public mo.Report Add(mo.Report report)
        {
            mo.Report result;
            try
            {
                if (report == null)
                    throw new ArgumentNullException("report");



                _reportRepo.Insert(report);

                result = report;
            }
            catch
            {

                throw;
            }

            return result;
        }

        public mo.Report GetById(int Id)
        {
            var query = from s in _reportRepo.Table
                        where s.Id == Id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

        public mo.Report GetLatestBySubId(int subId)
        {
            var query = from s in _reportRepo.Table
                        where s.SubscriptionId == subId
                        orderby s.CompletionTime descending
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }

        public bool Update(mo.Report report)
        {
            var result = false;
            try
            {
                if (report == null)
                    throw new ArgumentNullException("report");

                _reportRepo.Update(report);

                result = true;
            }
            catch 
            {
                throw;
            }

            return result;
        }

        public mo.Report GetLatestByDateBySubId(int subId, DateTime date)
        {
            var query = from s in _reportRepo.Table
                        where s.CompletionTime <= date && s.SubscriptionId == subId
                        orderby s.CompletionTime descending
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }
    }
}
