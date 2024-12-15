namespace Kultura.Domain.Entities
{
    public class SocialLink
    {
        public int Id { get; set; } 
        public string Platform { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
