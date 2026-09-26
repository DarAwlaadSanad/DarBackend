namespace DarV2.DTOs
{
    public class StudentFeeViewDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public Models.Gender? Gender { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public decimal RequiredAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateOnly? PaymentDate { get; set; }
        public bool IsExempted { get; set; }
        public string? ExemptionReason { get; set; }
        public bool IsPermanentlyExempted { get; set; }
    }
}
