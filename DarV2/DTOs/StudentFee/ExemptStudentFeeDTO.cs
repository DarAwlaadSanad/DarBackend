using System.ComponentModel.DataAnnotations;

namespace DarV2.DTOs
{
    public class ExemptStudentFeeDTO
    {
        [Required]
        public string Reason { get; set; }
    }
}
