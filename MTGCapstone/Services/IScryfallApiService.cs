namespace MTGCapstone.API.Services
{
    public interface IScryfallApiService
    {
        Task GetBulkDataSourcesAsync(CancellationToken cancellationToken);
        Task ImportRulingsAsync(CancellationToken cancellationToken);
        Task ImportCardsAsync(CancellationToken cancellationToken);




    }
}
