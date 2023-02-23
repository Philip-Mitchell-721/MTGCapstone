namespace MTGCapstone.API.Data.Models
{
    public class CardKeywordsLookUp
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int KeywordsLookUpId { get; set; }

        public Card? Card { get; set; }
        public KeywordsLookUp? KeywordsLookUp { get; set; }
    }
}