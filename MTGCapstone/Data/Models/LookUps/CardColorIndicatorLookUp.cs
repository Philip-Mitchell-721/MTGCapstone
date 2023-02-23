namespace MTGCapstone.API.Data.Models
{
    public class CardColorIndicatorLookUp
    {
        public int Id { get; set; }
        public int? CardId { get; set; }
        public int? CardFaceId { get; set; }
        public int ColorIndicatorLookUpId { get; set; }

        public CardFace? CardFace { get; set; }
        public Card? Card { get; set; }
        public ColorIndicatorLookUp? ColorIndicatorLookUp { get; set; }

    }
}