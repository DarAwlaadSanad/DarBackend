namespace DarV2.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
