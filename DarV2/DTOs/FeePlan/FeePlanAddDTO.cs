namespace DarV2.DTOs
{
    public class FeePlanAddDTO
    {
        public int GroupId { get; set; }
        public decimal Amount { get; set; }
        public DateOnly EffectiveFrom { get; set; }
    }
}
