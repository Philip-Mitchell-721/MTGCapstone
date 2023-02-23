namespace MTGCapstone.API.Data.Models
{
    public class CardColorIdentityLookUp
    {
        public int Id { get; set; }
        public int? CardId { get; set; }
        public int ColorIdentityLookUpId { get; set; }

        public Card? Card { get; set; }
        public ColorIdentityLookUp? ColorIdentityLookUp { get; set; }

    }
}