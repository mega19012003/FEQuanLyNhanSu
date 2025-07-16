using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEQuanLyNhanSu.ResponseModels
{
    public class LogStatusConfigs
    {
        public class LogStatusDto
        {
            public Guid Id { get; set; }
            public int enumId { get; set; }
            public string Name { get; set; } = null!;
            public double SalaryMultiplier { get; set; }
            public string? Note { get; set; }

            public Guid? CompanyId { get; set; }
            public string? CompanyName { get; set; }
            public bool IsSystemDefault { get; set; }
        }
    }
}
