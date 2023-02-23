namespace MTGCapstone.API.Data.Models
{
    public class CardFinishesLookUp
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public int FinishesLookUpId { get; set; }

        public Card? Card { get; set; } 
        public FinishesLookUp? FinishesLookUp { get; set; }
    }
}