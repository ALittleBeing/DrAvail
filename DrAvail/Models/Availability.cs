﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrAvail.Models
{
    public class Availability
    {
        [Microsoft.EntityFrameworkCore.Owned]
        public class Timings
        {
            //Morning start
            [NotMapped]
            public string MorningStartHour { get; set; }

            [NotMapped]
            public string MorningStartMinute { get; set; }


            [DataType(DataType.Time)]
            public DateTime MorningStartTime { get; set; }
            
            //Morning End
            [NotMapped]
            public string MorningEndHour { get; set; }

            [NotMapped]
            public string MorningEndMinute { get; set; }

            [DataType(DataType.Time)]
            public DateTime MorningEndTime { get; set; }

            //Evening start
            [NotMapped]
            public string EveningStartHour { get; set; }

            [NotMapped]
            public string EveningStartMinute { get; set; }

            [DataType(DataType.Time)]
            public DateTime EveningStartTime { get; set; }

            //Evening End
            [NotMapped]
            public string EveningEndHour { get; set; }

            [NotMapped]
            public string EveningEndMinute { get; set; }

            [DataType(DataType.Time)]
            public DateTime EveningEndTime { get; set; }
        }


        public int ID { get; set; }

        [Required]
        public string AvailabilityType { get; set; }

        [MaxLength(15)]
        public string Status { get; set; } = "Available";

        public Timings CommonDays { get; set; }

        public bool IsAvailableOnWeekend { get; set; } = false;

        public bool WeekendSameAsCommon { get; set; } = false;

        [RequireWhenAvailableOnWeekend]
        public Timings Weekends { get; set; }

        //for current availability

        [RequireWhenCurrent]
        public DateTime? CurrentStartDateTime { get; set; }

        [RequireWhenCurrent]
        public DateTime? CurrentEndDateTime { get; set; }

        public ContactPreference ContactPreference { get; set; } = ContactPreference.Always;

        public int? HospitalID { get; set; }

        public Hospital Hospital { get; set; }

    }

    #region Custom ValidationAttribute
    public class RequireWhenCurrentAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var availability = (Availability)validationContext.ObjectInstance;
            if (!string.IsNullOrEmpty(availability.AvailabilityType) && availability.AvailabilityType.Contains("Common"))
                return ValidationResult.Success;

            if (value == null) {

                var propertyInfo = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                return new ValidationResult($"{propertyInfo.Name} is required");

            }    
                return ValidationResult.Success;

        }
    }

    public class RequireWhenAvailableOnWeekendAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var availability = (Availability)validationContext.ObjectInstance;
            if (!availability.IsAvailableOnWeekend)
                return ValidationResult.Success;

            if (value == null)
            {
                //var propertyInfo = validationContext.ObjectType.GetProperty(validationContext.MemberName);
                return new ValidationResult("Weekend timing is required");
            }
            return ValidationResult.Success;

        }
    }
    #endregion
}
