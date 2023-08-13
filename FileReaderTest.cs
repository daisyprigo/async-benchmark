public class FileReaderTest
{
    private string[] testFileList;

    /// <summary>
    /// This controls how many files we will read concurrently, e.g. 3 different sizes of runs: 10, 100, 1000
    /// </summary>
    public int ConcurrentOperationCount;

    public int MaxIntervalBetweenOperationsMs;

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

                var bytes = File.ReadAllBytes(file);
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
                var bytes = await File.ReadAllBytesAsync(file, ct);
                return;
            });
    }
    private void ReadFilesConcurrently(Func<string, CancellationToken, Task> fileRead)
    {
        var rand = new Random();
        var filesToLoad = GetRandomTestFileSample();
        var runningTasks = new List<Task>(ConcurrentOperationCount);
        foreach (var file in filesToLoad)
        {
            var task = fileRead(file, CancellationToken.None);
            task.ConfigureAwait(false);
            runningTasks.Add(task);
            // Wait a little bit between read operations, to simulate a distribution of incoming requests over time
            if (MaxIntervalBetweenOperationsMs > 0)
            {
                Thread.Sleep(rand.Next(MaxIntervalBetweenOperationsMs));
            }
                
        }
        // wait for all read operations to complete
        Task.WaitAll(runningTasks.ToArray());
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
        return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
    }
}