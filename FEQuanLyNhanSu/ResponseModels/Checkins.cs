using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Models;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

namespace FEQuanLyNhanSu.Services
{
    public class Checkins
    {
        public class CheckinResultDto
        {
            public Guid CheckinId { get; set; }
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public DateTime CheckinTime { get; set; }
            public DateTime CheckoutTime { get; set; }
            public int? LogStatus { get; set; }
            public string Status { get; set; } 
            //public double SalaryPerDay { get; set; }
        }
        public class CheckinResponse
        {
            public string Message { get; set; }
            public CheckinResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
        public class UserWithCheckinsDto
        {
            public Guid UserId { get; set; }
            public string FullName { get; set; } = null!;
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string ImageUrl { get; set; }

            public ObservableCollection<CheckinResultDto> Checkins { get; set; } = new ObservableCollection<CheckinResultDto>();
        }
    }
}
