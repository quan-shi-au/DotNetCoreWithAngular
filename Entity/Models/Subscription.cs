using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class Subscription : BaseEntity
    {
        public Subscription()
        {
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public int EnterpriseClientId { get; set; }

        [Required]
        public int Product { get; set; }

        [Required]
        public int LicencingEnvironment { get; set; }

        [Required]
        public string BrandId { get; set; }

        [Required]
        public string Campaign { get; set; }

        [Required]
        public int SeatCount { get; set; }

        [Required]
        public string CoreAuthUsername { get; set; }

        [Required]
        public string CoreAuthPassword { get; set; }

        [Required]
        public string RegAuthUsername { get; set; }

        [Required]
        public string RegAuthPassword { get; set; }

        [Required]
        public string ClientDownloadLocation { get; set; }

        [Required]
        public string LicenceKey { get; set; }

        [Required]
        public bool Status { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }


        public DateTime? ModificationTime { get; set; }

          public DateTime? CancelationTime { get; set; }



    }
}
