﻿using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ent.manager.Entity.Model
{
    public class DeviceType : BaseEntity
    {
        public DeviceType()
        {
        }


        [Required]
        public int wId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string wName { get; set; }
    }
}
