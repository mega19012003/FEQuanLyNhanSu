using System;
using System.Text.Json.Serialization;
using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Models;

namespace FEQuanLyNhanSu.Services
{
    public class Checkins
    {
        public class CheckinResultDto
        {
            public Guid CheckinId { get; set; }
            public string Name { get; set; }
            public DateTime CheckinMorning { get; set; }
            public DateTime CheckoutMorning { get; set; }
            public DateTime CheckinAfternoon { get; set; }
            public DateTime CheckoutAfternoon { get; set; }
            public string? CheckinMorningStatus { get; set; }
            public string? CheckoutMorningStatus { get; set; }
            public string? CheckinAfternoonStatus { get; set; }
            public string? CheckoutAfternoonStatus { get; set; }
            public double SalaryPerDay { get; set; }
        }

        public class CreateCheckinDto
        {
            public Guid? userId { get; set; }
            //public Enums.LogStatus? LogStatus { get; set; }
            //public DateTime CheckinTime { get; set; } = DateTime.Now;
        }

        public class CreateCheckoutDto
        {

            public Guid? userId { get; set; }
            //public Enums.LogStatus? CheckoutAfternoonStatus { get; set; } 
            //public DateTime CheckoutTime { get; set; } = DateTime.Now;
        }

        public class UpdateCheckinDto
        {
            public Guid CheckinId { get; set; }
            public Enums.LogStatus CheckinMorningStatus { get; set; }
            public Enums.LogStatus CheckoutMorningStatus { get; set; }
            public Enums.LogStatus CheckinAfternoonStatus { get; set; }
            public Enums.LogStatus CheckoutAfternoonStatus { get; set; }
        }

        public class CheckinDetailDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string CheckinMorningStatus { get; set; }
            public string CheckoutMorningStatus { get; set; }
            public string CheckinAfternoonStatus { get; set; }
            public string CheckoutAfternoonStatus { get; set; }
            public double SalaryPerDay { get; set; }
            public DateTime TimeCheckin { get; set; }
        }
    }
}
