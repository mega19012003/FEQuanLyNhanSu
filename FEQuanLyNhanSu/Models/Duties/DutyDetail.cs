using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using FEQuanLyNhanSu.Models.Users;
using EmployeeAPI.Enums;
namespace FEQuanLyNhanSu.Models.Duties
{
    public class DutyDetail
    {
        [Key]
        public Guid DutyDetailId { get; set; } = Guid.NewGuid();
        public Guid DutyId { get; set; }
        public Duty Duty { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public DateOnly Deadline { get; set; }
        public DutyStatus Status { get; set; } = DutyStatus.NotStarted;
        public bool IsDeleted { get; set; } = false;
        public string Description { get; set; }
    }
}
