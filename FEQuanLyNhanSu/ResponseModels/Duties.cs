using FEQuanLyNhanSu.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

namespace FEQuanLyNhanSu.ResponseModels
{
    public static class Duties
    {
        public class DutyResultDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string AssignedBy { get; set; }
            public bool IsCompleted { get; set; }
            public List<DutyDetailResultDto> DutyDetails { get; set; } = new List<DutyDetailResultDto>();
        }
        public class DutyDetailResultDto
        {
            public Guid DutyDetailId { get; set; }
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public bool IsCompleted { get; set; }
            public string Description { get; set; }
        }
        public class DutyResponse
        {
            public string Message { get; set; }
            public DutyResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
        public class DetailResponse
        {
            public string Message { get; set; }
            public DutyDetailResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
