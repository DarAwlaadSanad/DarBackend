using System;
using System.ComponentModel.DataAnnotations;

namespace DarV2.DTOs.AttendanceLocation
{
    public class AttendanceLocationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusInMeters { get; set; }
        public bool IsActive { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAttendanceLocationDTO
    {
        [Required(ErrorMessage = "اسم الموقع مطلوب")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "خط العرض مطلوب")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "خط الطول مطلوب")]
        public double Longitude { get; set; }

        public double RadiusInMeters { get; set; } = 50.0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Address { get; set; }
    }

    public class UpdateAttendanceLocationDTO : CreateAttendanceLocationDTO
    {
        public int Id { get; set; }
    }
}
