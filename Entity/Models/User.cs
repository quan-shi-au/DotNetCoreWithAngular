using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class User : BaseEntity
    {
        public User()
        {
        }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Firstname { get; set; }

        [Required]
        public string Lastname { get; set; }

        [Required]
        public string UserId { get; set; }


        public string Role { get; set; }


        public int? PartnerId { get; set; }

  
        public int? EnterpriseId { get; set; }

        public string Domain { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }


        public DateTime? ModificationTime { get; set; }

    }
}
