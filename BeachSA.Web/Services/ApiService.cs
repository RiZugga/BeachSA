namespace BeachSA.Web.Services
{
    public class ApiService
    {
        public HttpClient ApiBeachSAStart()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://localhost:7137/api/");

            return client;

        }
    }
}
