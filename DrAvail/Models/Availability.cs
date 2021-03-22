using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrAvail.Models
{
    public class Availability
    {
        [Microsoft.EntityFrameworkCore.Owned]
        public class Timings
        {
            [DataType(DataType.Time)]
            public DateTime MorningStartTime { get; set; }

            [DataType(DataType.Time)]
            public DateTime MorningEndTime { get; set; }

            [DataType(DataType.Time)]
            public DateTime EveningStartTime { get; set; }

            [DataType(DataType.Time)]
            public DateTime EveningEndTime { get; set; }
        }


        public int ID { get; set; }

        [MaxLength(15)]
        public string Status { get; set; }

        public Timings CommonDays { get; set; }

        public bool IsAvailableOnWeekend { get; set; }

        public Timings Weekends { get; set; }

        //for current availability

        public DateTime CurrentStartDateTime { get; set; }
        public DateTime CurrentEndDateTime { get; set; }

        public ContactPreference ContactPreference { get; set; }

        public int? HospitalID { get; set; }

        public Hospital Hospital { get; set; }

    }

    
}
