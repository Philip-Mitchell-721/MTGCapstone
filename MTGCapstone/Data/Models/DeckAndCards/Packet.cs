namespace MTGCapstone.API.Data.Models
{
    public class Packet
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string? Name { get; set; }
        public bool IsPrivate { get; set; } = true;

        public List<PacketCard> Cards { get; set; } = new List<PacketCard>();
    }
}
