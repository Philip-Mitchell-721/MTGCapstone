namespace MTGCapstone.API.Data.Models
{
    public class ColorIdentityLookUp
    {
        public int Id { get; set; }
        public string? Value { get; set; } //{G}
        public string? FullValue { get; set; } //"Green"

        public List<CardColorIdentityLookUp> Cards { get; set; } = new List<CardColorIdentityLookUp>();

    }
}

//Cards.Include(card => card.Colors)
//        .Where(card => card.Colors
//            .Select(colors => colors.FullValue)
//                .Contains("Green"));

//Cards.SelectMany(card => card.Colors.Where(color => color.FullValue == "Green"));

//Cards.Include(card => card.Colors)
//        .Where(card => card.Colors.Where(color => color.FullValue == "Green"));

//ColorsLookUp.Include(c => c.Cards)
//    .Where(c => c.FullValue == "Green");

