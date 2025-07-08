using FEQuanLyNhanSu.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static FEQuanLyNhanSu.ResponseModels.AllowedIPs;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Auths
    {
        public class AuthDto
        {
            public Guid userId { get; set; }
            public String Username { get; set; }
            public string Fullname { get; set; }
            //public string Password { get; set; }
            public string RoleName { get; set; }
        }
        public class AuthResponse
        {
            public string Message { get; set; }
            public AuthDto Data { get; set; }
            public int StatusCode { get; set; }
        }
        public class LoginResult
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
        }
    }
}
