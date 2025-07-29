using EmployeeAPI.Enums;
using EmployeeAPI.Models;
using FEQuanLyNhanSu.Models.Users;
using System.Text.Json.Serialization;

namespace FEQuanLyNhanSu.Models.Duties
{
    public class Duty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public Guid AssignedById { get; set; }
        public User AssignedBy { get; set; }

        public DutyStatus Status { get; set; } = DutyStatus.NotStarted;
        public bool IsDeleted { get; set; } = false;
        public ICollection<DutyDetail> DutyDetails { get; set; } = new List<DutyDetail>();

        public Company Company { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
