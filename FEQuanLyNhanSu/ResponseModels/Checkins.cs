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
            //public Guid CheckinId { get; set; }
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
    }
}
