using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class UserData : BaseEntity
    {
        public UserData()
        {
        }

        [Required]
        public byte[] FirstName { get; set; }

        [Required]
        public byte[] LastName { get; set; }

        [Required]
        public string LicenceKey { get; set; }

        [Required]
        public string SeatKey { get; set; }

        public byte[] OptionalData { get; set; }

        public string DeviceType { get; set; }

        public string DeviceModel { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }

        public DateTime? ModificationTime { get; set; }

    }
}
