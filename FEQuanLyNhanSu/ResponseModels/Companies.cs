using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FEQuanLyNhanSu.Services.UserService.Users;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class Companies
    {
        public class CompanyResultDto
        {
            public Guid CompanyId { get; set; }
            public string? Name { get; set; }
            public string? Address { get; set; }
            public string? LogoUrl { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class CompanyResponse
        {
            public string Message { get; set; }
            public CompanyResultDto Data { get; set; }
            public int StatusCode { get; set; }
        }
    }
}
