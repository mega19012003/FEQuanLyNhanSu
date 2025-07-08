using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Models;
using System;
using System.Text.Json.Serialization;
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

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
        public class CheckinResultDto1
        {
            public Guid CheckinId { get; set; }
            public string Name { get; set; }
            public DateTime CheckinMorning { get; set; }
            public DateTime CheckoutMorning { get; set; }
            public DateTime CheckinAfternoon { get; set; }
            public DateTime CheckoutAfternoon { get; set; }
            public int? CheckinMorningStatus { get; set; }
            public int? CheckoutMorningStatus { get; set; }
            public int? CheckinAfternoonStatus { get; set; }
            public int? CheckoutAfternoonStatus { get; set; }
            public double SalaryPerDay { get; set; }
        }
        public class CheckinResponse
        {
            public string Message { get; set; }
            public CheckinResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
