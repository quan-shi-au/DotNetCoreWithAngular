using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model.Reporting
{
    public class DeviceManufacturerReport : BaseEntity
    {
        public DeviceManufacturerReport()
        {
        }

        [Required]
        public string Data { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }


    }
}
