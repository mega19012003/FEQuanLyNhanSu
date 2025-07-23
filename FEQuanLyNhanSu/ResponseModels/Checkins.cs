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
            public string CompanyName { get; set; }
            public string DepartmentName { get; set; }
            public string PositionName { get; set; }

            public List<CheckinResultDto> Checkins { get; set; } = new List<CheckinResultDto>();
        }
    }
}
