using Microsoft.EntityFrameworkCore;

namespace MTGCapstone.API.Data.Models
{
    public class Prices
    {
        public int Id { get; set; }
        public int CardId { get; set; }

        [Precision(18, 2)]
        public decimal? Usd { get; set; }

        [Precision(18, 2)]
        public decimal? UsdFoil { get; set; }

        [Precision(18, 2)]
        public decimal? UsdEtched { get; set; }

        [Precision(18, 2)]
        public decimal? Eur { get; set; }

        [Precision(18, 2)]
        public decimal? EurFoil { get; set; }

        [Precision(18, 2)]
        public decimal? Tix { get; set; }
    }

}
