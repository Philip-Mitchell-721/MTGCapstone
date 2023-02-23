namespace MTGCapstone.API.Data.Models
{
    public class FinishesLookUp 
    {
        public int Id { get; set; }
        public string? Value { get; set; }

        public List<CardFinishesLookUp> Cards { get; set; } = new List<CardFinishesLookUp>();
    }
}