namespace DarV2.DTOs
{
    public class UpdateStudentFeePaymentDTO
    {
        public decimal AmountPaid { get; set; }
        public DateOnly? PaymentDate { get; set; }
    }
}
