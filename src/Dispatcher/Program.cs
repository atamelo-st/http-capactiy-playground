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

                Console.Write("Sliding window size: ");
                int windowSize = int.TryParse(Console.ReadLine(), out int size) is true ? size : 1;

                Console.WriteLine("Posting to {0}", url);
                Console.WriteLine("Http version: {0}", client.DefaultRequestVersion);

                var tasks = new Task[windowSize];

                for (int i = 0; i < windowSize; i++)
                {
                    var response = client.PostAsync(url, null);
                    tasks[i] = response;
                }

                int tasksRemaining = taskCount;
                int successfullyRan = 0;

                while (tasksRemaining > 0)
                {
                    // TODO: highly inefficient, think of another approach for tracking/awaiting outstanding tasks
                    int finishedTask = Task.WaitAny(tasks);

                    try
                    {
                        await tasks[finishedTask];
                        successfullyRan++;
                        // NOTE: if fails due to free port exhaustion, we re-run the task 
                        tasksRemaining--;
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine(e.Message);
                    }

                    tasks[finishedTask] = client.PostAsync(url, null);
                }

                Console.WriteLine($"Successfully ran {successfullyRan} requests in parallel.");

            }
        }
    }
}