namespace DarV2.Models.Finance
{
    public class SalaryPayment
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaidAt { get; set; }
    }
}
