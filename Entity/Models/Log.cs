using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class Log : BaseEntity
    {
        public Log()
        {
        }

        public string Level { get; set; }
        public string Category { get; set; }
        public string Source { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }
    }
}
