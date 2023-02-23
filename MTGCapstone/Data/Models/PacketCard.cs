namespace MTGCapstone.API.Data.Models
{
    public class PacketCard 
    {
        public int Id { get; set; }
        public int? CardId { get; set; }
        public int PacketId { get; set; }

        public Card? Card { get; set; }
        public Packet? Packet { get; set; }

    }
}
