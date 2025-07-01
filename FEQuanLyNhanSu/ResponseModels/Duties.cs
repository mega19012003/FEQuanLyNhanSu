﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FEQuanLyNhanSu.Models;

namespace FEQuanLyNhanSu.ResponseModels
{
    public static class Duties
    {
        public class DutyResultDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public string AssignedBy { get; set; }
            public bool IsCompleted { get; set; }
            public List<DutyDetailResultDto> DutyDetails { get; set; } = new List<DutyDetailResultDto>();
        }
        public class DutyDetailResultDto
        {
            public Guid DutyDetailId { get; set; }
            public Guid UserId { get; set; }
            public string Name { get; set; }
            public bool IsCompleted { get; set; }
            public string Description { get; set; }
        }
        public class CreateDutyDto
        {
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public List<CreateDutyDetailDto> DutyDetails { get; set; } = new List<CreateDutyDetailDto>();
        }

        public class GetDutyDto
        {
            public Guid Id { get; set; }
            public List<CreateDutyDetailDto> DutyDetails { get; set; } = new List<CreateDutyDetailDto>();
        }
        public class UpdateDutyDto
        {
            public Guid Id { get; set; }

            public string Name { get; set; }
            //public bool IsCompleted { get; set; }
        }

        public class CreateDutyDetailDto
        {
            public Guid userId { get; set; }
            public string Description { get; set; }
        }
        public class UpdateDutyDetailDto
        {
            public Guid DutyDetailId { get; set; }
            public Guid userId { get; set; }
            public string Description { get; set; }
        }
    }
}
