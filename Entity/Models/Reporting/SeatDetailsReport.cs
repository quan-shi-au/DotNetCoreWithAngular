using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model.Reporting
{
    public class SeatDetailsReport : BaseEntity
    {
        public SeatDetailsReport()
        {
        }

        public string SeatKey { get; set; }

        public string DeviceName { get; set; }


        public string DeviceType { get; set; }

    
        public string FirstName { get; set; }

 
        public string LastName { get; set; }

   
        public string DeviceModel { get; set; }

      
        public string OsVersion { get; set; }


        public string OptionalData { get; set; }

       
        public DateTime ActivationDate { get; set; }

     
        public DateTime LastUpdateDate { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }

    }
}
