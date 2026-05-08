namespace DarV2.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
