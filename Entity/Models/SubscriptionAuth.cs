using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class SubscriptionAuth : BaseEntity
    {
        public SubscriptionAuth()
        {
        }

        [Required]
        public int SubscriptionId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Pin { get; set; }

        


    }
}
