using FEQuanLyNhanSu.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static FEQuanLyNhanSu.ResponseModels.Departments;

namespace FEQuanLyNhanSu.Services.UserService
{
    public class Users
    {
        public class UserResultDto
        {
            public Guid UserId { get; set; }
            public string? Fullname { get; set; }
            public string? Username { get; set; }
            public string RoleName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string DepartmentName { get; set; }
            public string PositionName { get; set; }
            public double? BasicSalary { get; set; }
            public bool IsActive { get; set; } 
            public string ImageUrl { get; set; }
        }
        public class UserResultUpdateDto
        {
            public Guid UserId { get; set; }
            public string? Fullname { get; set; }
            public string RoleName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? Address { get; set; }
            public string DepartmentName { get; set; }
            public Guid? DepartmentId { get; set; }
            public string PositionName { get; set; }
            public Guid? PositionId { get; set; }
            public bool IsActive { get; set; }
            public double? BasicSalary { get; set; }
            public string ImageUrl { get; set; }
        }
        public class UserResponse
        {
            public string Message { get; set; }
            public UserResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
