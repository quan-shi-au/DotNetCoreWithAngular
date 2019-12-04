using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model.Reporting
{
    public class Report : BaseEntity
    {
        public Report()
        {
        }

        [Required]
        public int Type { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }

        [Required]
        public int ReportProcessorRunId { get; set; }

 
        public DateTime? CompletionTime { get; set; }
    }
}
