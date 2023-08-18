
using Async_Benchmark;

//int mt;
//ThreadPool.GetMinThreads(out mt, out mt);
//ThreadPool.SetMinThreads(mt, mt);
//ThreadPool.SetMaxThreads(mt, mt);

// warm up the threadpool
Parallel.For(0, 8 * 2, (i) =>
    {
        Thread.SpinWait(1000);
        Thread.Sleep(100);
    }
);

// a random list of files that we will use for the test
var filesToLoad = Helpers.GetRandomTestFileSample(@"c:\windows\system32", 10000, false);

var results = new List<List<CounterSnapshot>>();

foreach (var useAsync in new bool[] { true, false})
{
    var t = new ParallelRequestSimulator();
    t.RequestsPerSec = 1000;
    results.Add(t.ExecuteTest(useAsync, filesToLoad));
}


var csvRows = new List<string>();
csvRows.Add("IsAsync,TimePoint,JobsNotStarted,JobsAtStage1,JobsAtStage2,JobsAtStage3,JobsCompleted");

foreach (var runResults in results)
{

    Console.WriteLine("Run results:");
    foreach (var snapshot in runResults)
    {
        Console.WriteLine(snapshot);
        csvRows.Add($"{snapshot.IsAsync},{snapshot.TimePoint},{snapshot.JobsNotStarted},{snapshot.JobsAtStage1},{snapshot.JobsAtStage2},{snapshot.JobsAtStage3},{snapshot.JobsCompleted},{snapshot.AvailableWorkerThreads},{snapshot.AvailableCompletionThreads}");
    }
}

File.WriteAllLines("output.csv", csvRows);

return;
var x = new FileReaderTest();
x.ConcurrentOperationCount = 100000;
x.MaxIntervalBetweenOperationsMs = 0;

//var t = 8;
//ThreadPool.SetMaxThreads(t,t);
//ThreadPool.SetMinThreads(t,t);

x.ReadFilesSync();
return;








