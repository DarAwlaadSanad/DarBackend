namespace DarV2.DTOs
{
    public class FeePlanViewDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
    }
}
