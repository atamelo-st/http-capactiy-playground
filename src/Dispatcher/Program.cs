namespace Dispatcher;

public class Program
{
    public static async Task Main(string[] args)
    {
        string url = "http://localhost:5281/WeatherForecast/callback";

        if (args.Length > 0 && string.IsNullOrWhiteSpace(args[0]) is false)
        {
            url = args[0];
        }

        using (HttpClient client = new())
        {
            while (true)
            {
                Console.Write("Number of requests: ");
                if (int.TryParse(Console.ReadLine(), out int taskCount) is false) break;

                Console.WriteLine("Posting to {0}", url);
                Console.WriteLine("Http version: {0}", client.DefaultRequestVersion);

                // TODO: highly inefficient, think of another approach for tracking/awaiting outstanding tasks
                var tasks = new Task[taskCount];

                for (int i = 0; i < taskCount; i++)
                {
                    var response = client.PostAsync(url, null);
                    tasks[i] = response;
                }

                TaskCompletionSource neverCompletes = new();

                int tasksRemaining = taskCount;
                int successfullyRan = 0;
                 
                while (tasksRemaining-- > 0)
                {
                    int finishedTask = Task.WaitAny(tasks);

                    try
                    {
                        await tasks[finishedTask];
                        successfullyRan++;
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    tasks[finishedTask] = neverCompletes.Task;
                }

                Console.WriteLine($"Successfully ran {successfullyRan} requests in parallel.");
            }
        }
    }
}