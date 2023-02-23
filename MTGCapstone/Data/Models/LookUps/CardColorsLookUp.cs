namespace MTGCapstone.API.Data.Models
{
    public class CardColorsLookUp
    {
        public int Id { get; set; }
        public int? CardId { get; set; }
        public int? CardFaceId { get; set; }
        public int ColorsLookUpId { get; set; }

        public Card? Card { get; set; }
        public CardFace? CardFace { get; set; }
        public ColorsLookUp? ColorsLookUp { get; set; }

    }
}