using FEQuanLyNhanSu.Models;
using System.Collections.ObjectModel;
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
            public DateOnly StartDate { get; set; }
            public DateOnly EndDate { get; set; }
            public Guid AssignedById { get; set; }
            public string AssignedBy { get; set; }
            public string AssignImageUrl { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsDeleted { get; set; }
            public ObservableCollection<DutyDetailResultDto> DutyDetails { get; set; } = new ObservableCollection<DutyDetailResultDto>();
            public Guid CompanyId { get; set; }
            public string CompanyName { get; set; }
        }
        public class DutyDetailResultDto
        {
            public Guid DutyDetailId { get; set; }
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public string UserImageUrl { get; set; }
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
