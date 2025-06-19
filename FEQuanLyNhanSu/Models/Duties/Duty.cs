﻿using System.Text.Json.Serialization;
using FEQuanLyNhanSu.Models.Users;

namespace FEQuanLyNhanSu.Models.Duties
{
    public class Duty //: BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }

        public Guid AssignedById { get; set; } 
        public User AssignedBy { get; set; }

        public bool IsCompleted { get; set; } = false; 
        public bool IsDeleted { get; set; } = false;
        public ICollection<DutyDetail> DutyDetails { get; set; } = new List<DutyDetail>();
        //public string note { get; set; } = string.Empty;
    }
}
