namespace DarV2.Models
{
    public class Phone
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
