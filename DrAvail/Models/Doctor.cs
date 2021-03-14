﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DrAvail.Models
{
    public class Doctor
    {

        public int ID { get; set; }

        [Required]
        [MinLength(3), MaxLength(60)]
        public string Name { get; set; }

        //Need to be unique
        [Required]
        [MaxLength(20)]
        public string RegNumber { get; set; }

        [Required]
        public string Speciality { get; set; }

        [Required]
        public string Degree { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public Practice Practice { get; set; }

        [Required]
        [Column(TypeName ="decimal(4,2)")]
        public decimal Experience { get; set; }

        [Required]
        public bool IsVerified { get; set; }

        public string Summary { get; set; }
                
        [Required]
        public string City { get; set; }

        [Required]
        public District District { get; set; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }
        
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public int HospitalID { get; set; }
        public virtual Hospital Hospital { get; set; }

        [ForeignKey(nameof(CommonAvailability))]
        public int CommonAvaliabilityID { get; set; }

        public virtual Avaliability CommonAvailability { get; set; }


        [ForeignKey(nameof(CurrentAvailability))]
        public int? CurrentAvaliabilityID { get; set; }

        public virtual Avaliability CurrentAvailability { get; set; }
    }

    
    #region Enums

    public enum Gender
    {
        Male = 0,
        Female
    }

    public enum Practice
    {
        Government =  0,
        Private
    }
    public enum Speciality
    {
        [Display(Name = "Siddha", Description = "The Siddha system is based on a combination of ancient medicinal practices and spiritual disciplines as well as alchemy and mysticism")]
        Siddha,

        [Display(Name = "Allergist", Description = "Specializes in determining food and environmental allergies")]
        Allergist,

        [Display(Name = "Anesthesiologist", Description = "Specializes in pain prevention during surgery")]
        Anesthesiologist,

        [Display(Name = "Cardiologist", Description = "Heart specialist")]
        Cardiologist,

        [Display(Name = "Chiropractor", Description = "Back specialist")]
        Chiropractor,

        [Display(Name = "Dentist", Description = "Tooth specialist")]
        Dentist,

        [Display(Name = "Dermatologist", Description = "Skin specialist")]
        Dermatologist,

        [Display(Name = "Fertility Specialist", Description = "Helps people who have difficulty getting pregnant")]
        FertilitySpecialist,

        [Display(Name = "Gynecologist", Description = "Specializes in women's needs")]
        Gynecologist,

        [Display(Name = "Massage Therapist", Description = "Specializes in muscle relaxation")]
        MassageTherapist,

        [Display(Name = "Naturopath", Description = "Specializes in natural cures and remedies")]
        Naturopath,

        [Display(Name = "Neurologist", Description = "Brain specialist")]
        Neurologist,

        [Display(Name = "Obstetrician", Description = "Specialist for pregnant women")]
        Obstetrician,

        [Display(Name = "Occupational Therapist", Description = "Specializes in workplace health")]
        OccupationalTherapist,

        [Display(Name = "Oncologist", Description = "Tumour specialist, including cancer")]
        Oncologist,

        [Display(Name = "Ophthalmologist", Description = "Specializes in eye diseases")]
        Ophthalmologist,

        [Display(Name = "Pediatrician", Description = "Specialist for babies and children")]
        Pediatrician,

        [Display(Name = "Physical Therapist", Description = "Specializes in the body's movement")]
        PhysicalTherapist,

        [Display(Name = "Podiatrist", Description = "Foot specialist")]
        Podiatrist,

        [Display(Name = "Psychiatrist", Description = "Specialist in mental health")]
        Psychiatrist,

        [Display(Name = "Radiologist", Description = "Specializes in imaging tests")]
        Radiologist,

        [Display(Name = "General Surgeons", Description = "Operate on all parts of your body")]
        GeneralSurgeons,

        [Display(Name = "General Physician", Description = "Highly trained specialists who provide a range of non-surgical health care to adult patients")]
        GeneralPhysician
    }

    public enum ContactPreference
    {
        Always,
        EmergencyOnly,
        Never
    }
    #endregion

}
