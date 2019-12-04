using ent.manager.Data.Repositories;
using mo = ent.manager.Entity.Model.Reporting;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ent.manager.Services.Reporting.ReportProcessorRun
{
    public class ReportProcessorRunService : IReportProcessorRunService
    {

     
        private readonly IRepository<mo.ReportProcessorRun> _reportProcessorRunRepo;
       
 
        public ReportProcessorRunService(IRepository<mo.ReportProcessorRun> userRepo)
        {
            _reportProcessorRunRepo = userRepo;
        }

        public bool Add(mo.ReportProcessorRun reportProcessorRun)
        {
            var result = false;
            try
            {
                if (reportProcessorRun == null)
                    throw new ArgumentNullException("reportProcessorRun");

                reportProcessorRun.StartRunTime = DateTime.UtcNow;

                _reportProcessorRunRepo.Insert(reportProcessorRun);

                result = true;
            }
            catch 
            {

                throw;
            }
           
            return result;
        }
 

        

        public mo.ReportProcessorRun GetLatestRun()
        {
            var query = from s in _reportProcessorRunRepo.Table
                        orderby s.StartRunTime descending
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }
       
        public mo.ReportProcessorRun GetById(int Id)
        {
            var query = from s in _reportProcessorRunRepo.Table
                        where s.Id == Id
                        select s;
            var result = query.FirstOrDefault();
            return result;
        }


        public bool  Update(mo.ReportProcessorRun reportProcessorRun)
        {
            var result = false;
            try
            {
                if (reportProcessorRun == null)
                    throw new ArgumentNullException("reportProcessorRun");

                _reportProcessorRunRepo.Update(reportProcessorRun);

                result = true;
            }
            catch 
            {
                throw;
            }

            return result;
        }

        public IEnumerable<mo.ReportProcessorRun> GetAll()
        {
            var query = from s in _reportProcessorRunRepo.Table
                        orderby s.Id
                        select s;
            var result = query.ToList();
            return result;
        }
    }
}
