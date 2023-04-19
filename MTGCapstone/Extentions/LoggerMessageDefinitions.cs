namespace MTGCapstone.API.Extentions.LoggerMessages
{
    public static partial class LoggerMessageDefinitions
    {

        [LoggerMessage(0, LogLevel.Information, 
            "User {userId} does not have permission to edit deck {deckId}")]
        public static partial void LogNotOwner(this ILogger logger, int userId, int deckId);
    }
}
