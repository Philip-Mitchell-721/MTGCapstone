namespace MTGCapstone.API.Data.Models
{
    public class PurchaseUris
    {
        public int Id { get; set; }
        public int CardId { get; set; }

        public string? Tcgplayer { get; set; }
        public string? Cardmarket { get; set; }
        public string? Cardhoarder { get; set; }
    }

}
