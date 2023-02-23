namespace MTGCapstone.API.Data.Models
{
    public class MultiverseIdsLookUp
    {
        public int Id { get; set; }
        public int? Value { get; set; }

        public int CardId { get; set; }
        public Card? Card { get; set; }
    }
}