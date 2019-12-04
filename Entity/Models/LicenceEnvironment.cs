using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class LicenceEnvironment : BaseEntity
    {
        public LicenceEnvironment()
        {
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public int wId { get; set; }

        [Required]
        public string CoreURL { get; set; }

        [Required]
        public string RegURL { get; set; }



    }
}
