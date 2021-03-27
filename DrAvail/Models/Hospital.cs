using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DrAvail.Models
{
    public class Hospital
    {
        public int ID { get; set; }

        [Required]
        [Display(Name ="Hospital Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Hospital Type")]
        public HospitalType Type { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public District District { get; set; }

        [Required]
        [DataType(DataType.PostalCode)]
        public int Pincode { get; set; }


        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string EmailId { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNo { get; set; }

        public ICollection<Doctor> Doctors { get; set; }
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
