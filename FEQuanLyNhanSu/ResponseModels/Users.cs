﻿using FEQuanLyNhanSu.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FEQuanLyNhanSu.Services.UserService
{
    public class Users
    {
        public class UserDto //: BaseDto
        {
            public Guid userId { get; set; }
            public string? Fullname { get; set; }
            [JsonIgnore]
            public RoleType? Role { get; set; } //= RoleType.Employee;
            public string RoleName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            [JsonIgnore]
            public Guid? DepartmentId { get; set; }
            public string DepartmentName { get; set; }
            [JsonIgnore]
            public Guid? PositionId { get; set; }
            public string PositionName { get; set; }
            public double? BasicSalary { get; set; }
            public string ImageUrl { get; set; }
        }

        public class AdminUpdateDto 
        {
            [Required]
            public Guid UserId { get; set; }
            public string? Fullname { get; set; }
            public RoleType? Role { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public Guid? DepartmentId { get; set; }
            public Guid? PositionId { get; set; }
            //[Required]
            public double? BasicSalary { get; set; }
            //public IFormFile? ImageUrl { get; set; }
            public byte[]? Imageurl { get; set; }
            [Required]
            public bool IsActive { get; set; }
        }

        public class ManagerUpdateDto 
        {
            [Required]
            public Guid UserId { get; set; }
            public string? Fullname { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public Guid? PositionId { get; set; }
            //[Required]
            public double? BasicSalary { get; set; }
            //public IFormFile? ImageUrl { get; set; }
            public byte[]? ImageUrl { get; set; }
            [Required]
            public bool IsActive { get; set; } = true;
        }

        /*public class UserFilter
        {
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public double BasicSalary { get; set; }
            public string ImageUrl { get; set; }
        }*/

    }
}
