namespace DarV2.Models
{
    public class StudentFee
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public decimal RequiredAmount { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal AmountPaid { get; set; }
        public DateOnly? PaymentDate { get; set; }
    }
}
