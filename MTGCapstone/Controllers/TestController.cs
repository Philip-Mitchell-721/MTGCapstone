using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MTGCapstone.API.DbContexts;
using MTGCapstone.API.Services;

namespace MTGCapstone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IScryfallApiService _scryfallApiService;

        public TestController(IScryfallApiService scryfallApiService)
        {
            _scryfallApiService = scryfallApiService 
                ?? throw new ArgumentNullException(nameof(scryfallApiService));
           
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            
            //await _scryfallApiService.GetBulkDataSourcesAsync(cancellationSource.Token);
            await _scryfallApiService.ImportRulingsAsync(cancellationSource.Token);

            return Ok();
        }
    }
}
