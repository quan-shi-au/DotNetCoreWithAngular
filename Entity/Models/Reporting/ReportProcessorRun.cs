using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model.Reporting
{
    public class ReportProcessorRun : BaseEntity
    {
        public ReportProcessorRun()
        {
        }

        [Required]
        public int Status { get; set; }

        [Required]
        public int ReportsCount { get; set; }

        [Required]
        public DateTime StartRunTime { get; set; }

        public DateTime? EndRunTime { get; set; }

    }
}
