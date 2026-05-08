namespace DarV2.DTOs
{
    public class StudentFeeViewDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal RequiredAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public DateOnly? PaymentDate { get; set; }
    }
}
