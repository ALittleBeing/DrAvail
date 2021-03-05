﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DrAvail.Models
{
    public class Hospital
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public HospitalType Type { get; set; }
        
        [Required]
        public string Address { get; set; }
        
        [Required]
        public string City { get; set; }
        
        [Required]
        public District District { get; set; }

    }

    public enum HospitalType
    {
        [Display(Name = "General Medical & Surgical Hospitals", Description = "General Medical & Surgical Hospitals")]
        General,

        [Display(Name = "Specialty Hospitals", Description = "Specialty Hospitals")]
        Specialty,

        [Display(Name = "Clinics", Description = "Clinics")]
        Clinic,

        [Display(Name = "Psychiatric Hospitals", Description = "Psychiatric Hospitals")]
        Psychiatric,

        [Display(Name = "Teaching Hospitals", Description = "Teaching Hospitals")]
        Teaching
    }
}
