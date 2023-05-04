using Microsoft.EntityFrameworkCore;

namespace MTGCapstone.API.Data.ViewModels
{
    public class PricesVM
    {
        public decimal? Usd { get; set; }

        public decimal? UsdFoil { get; set; }

        public decimal? UsdEtched { get; set; }

        public decimal? Eur { get; set; }

        public decimal? EurFoil { get; set; }

        public decimal? Tix { get; set; }
    }
}