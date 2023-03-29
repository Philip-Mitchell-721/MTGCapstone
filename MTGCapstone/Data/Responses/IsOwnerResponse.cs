namespace MTGCapstone.API.Data.Responses
{
    public class IsOwnerResponse : BaseResponse
    {
        public bool IsOwner { get; set; } = true;
        public bool DeckExists { get; set; } = true;
    }
}
