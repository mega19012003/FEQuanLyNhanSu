﻿using FEQuanLyNhanSu.Enums;
using FEQuanLyNhanSu.Models.Users;

namespace FEQuanLyNhanSu.Models.Checkins
{
    public class Checkin
    {
        public Guid CheckinId { get; set; }
        public Guid UserId { get; set; }
        public User Users { get; set; }
        public DateTime CheckinTime { get; set; }
        public DateTime CheckoutTime { get; set; }
        public Enums.LogStatus LogStatus { get; set; }
        public bool IsDeleted { get; set; }
        public string DeviceInfo { get; set; } = string.Empty;
        public string CheckinIP { get; set; }
        public string? CheckoutIP { get; set; }
        public double TotalTime { get; set; }
        public string Note { get; set; } = string.Empty;
    }

}
