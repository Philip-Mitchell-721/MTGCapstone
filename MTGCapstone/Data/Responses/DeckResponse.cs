using MTGCapstone.API.Data.Models;

namespace MTGCapstone.API.Data.Responses
{
    public class DeckResponse : BaseResponse
    {
        public bool IsOwner { get; set; }
        public bool DeckExists { get; set; }
        public Deck? Deck { get; set; }
    }
}
