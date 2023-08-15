using Async_Benchmark;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser(true)]
[ThreadingDiagnoser]
public class FileReader
{
    private string[] testFileList;

    /// <summary>
    /// This controls how many files we will read concurrently, e.g. 3 different sizes of runs: 10, 100, 1000
    /// </summary>
    //[Params(10, 100, 1000)]
    [Params(100)]
    public int ConcurrentOperationCount;

    [Params(100)]
    public int MaxIntervalBetweenOperationsMs;

    public int MinOperationDurationMs;

    public int SimulatedProcessingTimeInIterations;

    public void SimulateSyncIOOperations()
    {
        ExecuteConcurrentIO(SimulatedSyncIOOperation);
    }
    public void SimulateASyncIOOperations()
    {
        ExecuteConcurrentIO(SimulatedASyncIOOperation);
    }
    private async Task SimulatedSyncIOOperation()
    {
        // simulate some processing on the CPU
        Thread.SpinWait(SimulatedProcessingTimeInIterations);
        // simulate blocking the thread for an outbound IO call (e.g. reading form disk, calling an external process)
        await Task.Run(() => Thread.Sleep(MinOperationDurationMs)).ConfigureAwait(false);
        // simulate some post-processing on the CPU
        Thread.SpinWait(SimulatedProcessingTimeInIterations);
    }
    private async Task SimulatedASyncIOOperation()
    {
        // simulate some processing on the CPU
        Thread.SpinWait(SimulatedProcessingTimeInIterations);
        // simulate making an outbound IO call (e.g. reading form disk, calling an external process) without blocking the thread
        await Task.Delay(MinOperationDurationMs).ConfigureAwait(false);
        // simulate some post-processing on the CPU
        Thread.SpinWait(SimulatedProcessingTimeInIterations);
    }

   

    private void ExecuteConcurrentIO(Func<Task> ioOperation)
    {
        var rand = new Random();
        var runningTasks = new List<Task>(ConcurrentOperationCount);
        for (int i = 0; i < ConcurrentOperationCount; i++)
        {
            var task = ioOperation();
            runningTasks.Add(task);
            // Wait a little bit between read operations, to simulate a distribution of incoming requests over time
            if (MaxIntervalBetweenOperationsMs > 0)
            {
                Thread.Sleep(MaxIntervalBetweenOperationsMs);
            }

        }
        // wait for all read operations to complete
        Task.WaitAll(runningTasks.ToArray());
    }

    /// <summary>
    /// Reads files concurrently, using ThreadPool threads, using a synchronous read.
    /// </summary>
    [Benchmark]
    public void ReadFilesSync()
    {
        ReadFilesConcurrently((file, ct) =>
        {
            return Task.Run(() => File.ReadAllBytes(file));
        });
    }

    /// <summary>
    /// Reads files concurrently, using ThreadPool threads, using an asynchronous read.
    /// </summary>
    [Benchmark]
    public void ReadFilesASync()
    {
        ReadFilesConcurrently(File.ReadAllBytesAsync);
    }
    private void ReadFilesConcurrently(Func<string, CancellationToken, Task<byte[]>> fileRead)
    {
        var rand = new Random();

        var filesToLoad = Helpers.GetRandomTestFileSample(@"c:\windows\system32", ConcurrentOperationCount);
        var runningTasks = new List<Task>(ConcurrentOperationCount);
        foreach (var file in filesToLoad)
        {
            var task = fileRead(file, CancellationToken.None);
            task.ConfigureAwait(false);
            runningTasks.Add(task);
            // Wait a little bit between read operations, to simulate a distribution of incoming requests over time
            if (MaxIntervalBetweenOperationsMs > 0)
            {
                Task.Delay(rand.Next(MaxIntervalBetweenOperationsMs)).Wait();
            }
                
        }
        // wait for all read operations to complete
        Task.WaitAll(runningTasks.ToArray());
    }
    
}