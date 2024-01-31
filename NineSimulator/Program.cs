namespace NineSimulator;

using NineSimulator.Simulator;
using Libplanet.Crypto;

public class Program
{
    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    static async Task MainAsync(string[] args)
    {
        var arenaData = new ArenaDataReader().LoadArenaData();
        var bulkSimulator = new BulkSimulator();
        var tasks = new List<Task>();
        int maxConcurrentTasks = 100;
        var semaphore = new SemaphoreSlim(maxConcurrentTasks);

        foreach (var arena1 in arenaData)
        {
            foreach (var arena2 in arenaData)
            {
                for (int i = 0; i < 10000; i++)
                {
                    await semaphore.WaitAsync();

                    var task = bulkSimulator.Execute(arena1, arena2).ContinueWith(t =>
                    {
                        semaphore.Release();
                    });

                    tasks.Add(task);
                }
            }
        }

        await Task.WhenAll(tasks);
    }
}
