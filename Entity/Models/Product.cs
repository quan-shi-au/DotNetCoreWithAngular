using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class Product : BaseEntity
    {
        public Product()
        {
        }

        [Required]
        public string Name { get; set; }

        [Required]
        public int wId { get; set; }

        public string Codes { get; set; }

    }
}
