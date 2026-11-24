namespace BeachSA.Web.Services
{
    public class ApiService
    {
        public HttpClient ApiBeachSAStart()
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("http://localhost:5268/api/");

            return client;

        }
    }
}
