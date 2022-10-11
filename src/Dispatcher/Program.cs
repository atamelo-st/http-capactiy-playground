namespace Dispatcher;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        using (HttpClient client = new())
        {
            client.BaseAddress = new("http://localhost:5281");

            var response = await client.PostAsync("WeatherForecast/callback", null);

            Console.WriteLine(response.StatusCode);
        }
    }
}