using System.Net.Http.Headers;

namespace MTGCapstone.API.Services
{
    public class ScryfallClient
    {

        public ScryfallClient(HttpClient client)
        {
            
            client.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding
                .Add(new StringWithQualityHeaderValue("gzip"));
            Client = client 
                ?? throw new ArgumentNullException(nameof(client));
        }

        public HttpClient Client { get; }


    }
}
