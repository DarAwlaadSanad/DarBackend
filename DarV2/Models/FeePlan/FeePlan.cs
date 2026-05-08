namespace DarV2.Models
{
    public class FeePlan
    {
        public int Id { get; set; }
        
        public decimal Amount { get; set; }

        public DateOnly EffectiveFrom { get; set; }

        public DateOnly? EffectiveTo { get; set; }

        public bool IsActive { get; set; }


        public Group Group { get; set; }
        public int GroupId { get; set; }
    }
}
