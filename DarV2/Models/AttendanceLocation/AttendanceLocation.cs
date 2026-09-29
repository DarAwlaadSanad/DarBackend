using System;
using System.ComponentModel.DataAnnotations;

namespace DarV2.Models.AttendanceLocation
{
    public class AttendanceLocation
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double RadiusInMeters { get; set; } = 50.0;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
