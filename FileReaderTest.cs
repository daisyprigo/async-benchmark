using System.Diagnostics;

public class FileReaderTest
{
    private string[] testFileList;

    /// <summary>
    /// This controls how many files we will read concurrently, e.g. 3 different sizes of runs: 10, 100, 1000
    /// </summary>
    public int ConcurrentOperationCount;

    public int MaxIntervalBetweenOperationsMs;

    private int fileCounter = 0;

    public void Setup()
    {
        testFileList = GetTestFileList(@"c:\Windows\System32\");
    }

    /// <summary>
    /// Reads files concurrently, using ThreadPool threads, using a synchronous read.
    /// </summary>
    public void ReadFilesSync()
    {
        ReadFilesConcurrently((file, ct) =>
        {
            return Task.Run(() => {
                // synchronously read all the bytes from disk, blocking the thread
                var bytes = File.ReadAllBytes(file);
                var count = Interlocked.Increment(ref fileCounter);
                Console.WriteLine($"{count} - file read: {file}, bytes: {bytes.Length}");
                return;
                });
        });
    }

    /// <summary>
    /// Reads files concurrently, using ThreadPool threads, using an asynchronous read.
    /// </summary>
    public void ReadFilesASync()
    {
        ReadFilesConcurrently(async (file, ct) =>
            {
                // asynchronously read all the bytes, but do not block the thread while we "await"
                var bytes = await File.ReadAllBytesAsync(file, ct);
                var count = Interlocked.Increment(ref fileCounter);
                Console.WriteLine($"{count} - file read: {file}, bytes: {bytes.Length}");
                return;
            });
    }
    private void ReadFilesConcurrently(Func<string, CancellationToken, Task> fileReadImplementation)
    {
        var rand = new Random();
        // a random list of files that we will use for the test
        var filesToLoad = GetRandomTestFileSample();
        // we store all the tasks that we launched here
        var runningTasks = new List<Task>(ConcurrentOperationCount);

        var sw = Stopwatch.StartNew();
        foreach (var file in filesToLoad)
        {
            // here we are only "launching" the read operation, but we are not waiting for it to complete
            // how the operation will be executed (sync vs async) will be determined by the implementation that is passed to us as "fileRead"
            var task = fileReadImplementation(file, CancellationToken.None);
            // ignore for now
            task.ConfigureAwait(false);
            runningTasks.Add(task);
            // Wait a little bit between read operations, to simulate a distribution of incoming requests over time
            if (MaxIntervalBetweenOperationsMs > 0)
            {
                Thread.Sleep(rand.Next(MaxIntervalBetweenOperationsMs));
            }
                
        }
        Console.WriteLine($"Finished launching all file read ops, took: {sw.Elapsed.TotalSeconds} seconds.");
        sw.Restart();
        // wait for all read operations to complete
        Task.WaitAll(runningTasks.ToArray());
        Console.WriteLine($"Finished waiting for all tasks to complete, took: {sw.Elapsed.TotalSeconds} seconds.");
    }
    /// <summary>
    /// Gets a small set of random files to use for a test run
    /// </summary>
    /// <returns></returns>
    private IEnumerable<string> GetRandomTestFileSample()
    {
        var testFileSet = new List<string>(ConcurrentOperationCount);
        var rand = new Random();
        for (int i = 0;i< ConcurrentOperationCount;i++)
        {
            testFileSet.Add(testFileList[rand.Next(testFileList.Length)]);
            //testFileSet.Add(testFileList[i]);
        }
        return testFileSet;
    }
    /// <summary>
    /// Gets a full list of files from some folder we want to use as source of test files to read
    /// </summary>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    private static string[] GetTestFileList(string folderPath)
    {
        return Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly);
    }
}