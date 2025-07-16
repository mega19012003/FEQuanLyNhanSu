using EmployeeAPI.Models;
using FEQuanLyNhanSu.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FEQuanLyNhanSu.Models.Configs
{
    public class LogStatusConfig
    {
        public Guid Id { get; set; }
        public int enumId { get; set; }
        public string Name { get; set; } = null!;

        public double SalaryMultiplier { get; set; }

        public string? Note { get; set; }

        public Guid? CompanyId { get; set; }
        public bool IsSystemDefault { get; set; } // true=template, false=instance của công ty
        public Company Company { get; set; }
    }

}
