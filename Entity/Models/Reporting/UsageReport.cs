using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model.Reporting
{
    public class UsageReport : BaseEntity
    {
        public UsageReport()
        {
        }

        [Required]
        public int Available { get; set; }

        [Required]
        public int Used { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }

 

    }
}
