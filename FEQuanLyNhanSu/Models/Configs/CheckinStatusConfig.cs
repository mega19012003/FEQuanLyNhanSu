using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FEQuanLyNhanSu.Enums;

namespace FEQuanLyNhanSu.Models.Configs
{
    public class CheckinStatusConfig
    {
        /*[Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] */// 👈 Bắt buộc để EF không tự tăng Id
        public int Id { get; set; } 

        public string Name { get; set; } = null!; 

        public double SalaryMultiplier { get; set; } 

        public string? Note { get; set; } 
    }
}
