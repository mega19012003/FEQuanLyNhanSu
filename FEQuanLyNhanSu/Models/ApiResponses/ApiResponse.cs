using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEQuanLyNhanSu.Models.ApiResonses
{
    public class ApiResponse
    {
        public string Message { get; set; }
        public string Data { get; set; }
        public int StatusCode { get; set; }
    }

}
