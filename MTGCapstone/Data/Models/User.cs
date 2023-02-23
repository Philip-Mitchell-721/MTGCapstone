using Microsoft.AspNetCore.Identity;
using System.Net.Sockets;

namespace MTGCapstone.API.Data.Models
{
    public class User //: IdentityUser
    {
        public User(string userName, string password, string emailAddress)
        {
            UserName = userName;
            Password = password;
            EmailAddress = emailAddress;
        }
        //TODO: Migrate/Update database.  UserName, Password, and EmailAddress now required

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string? Bio { get; set; }
        public List<Like> Likes { get; set; } = new List<Like>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public List<Deck> Decks { get; set; } = new List<Deck>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Packet> Packets { get; set; } = new List<Packet>();

        public int? PreferredPriceUnitLookupId { get; set; }
        public PreferredPriceUnitLookup? PreferredPriceUnitLookup { get; set; }
        public int? PreferredMarketLookupId { get; set; }
        public PreferredMarketLookup? PreferredMarketLookup { get; set; }

    }
}
