using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class Partner : BaseEntity
    {
        public Partner()
        {
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }


        public DateTime? ModificationTime { get; set; }

    }
}
