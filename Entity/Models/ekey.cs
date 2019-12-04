using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class EKey : BaseEntity
    {
        public EKey()
        {
        }

        [Required]
        public string Key { get; set; }

        [Required]
        public string IV { get; set; }

        [Required]
        public int EnterpriseId { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }
 

    }
}
