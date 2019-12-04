using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model.Reporting
{
    public class ReportType : BaseEntity
    {
        public ReportType()
        {
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public int wId { get; set; }
 

    }
}
