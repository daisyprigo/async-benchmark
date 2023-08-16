
using Async_Benchmark;

var mt = 8;

ThreadPool.SetMinThreads(mt, mt);
ThreadPool.SetMaxThreads(mt, mt);

// a random list of files that we will use for the test
var filesToLoad = Helpers.GetRandomTestFileSample(@"c:\windows\system32", 1000, false);

var results = new List<List<CounterSnapshot>>();

foreach (var useAsync in new bool[] { true, false})
{
    var t = new ParallelRequestSimulator();
    t.RequestsPerSec = 100;
    results.Add(t.ExecuteTest(useAsync, filesToLoad));
}

foreach (var runResults in results)
{

    Console.WriteLine("Run results:");
    foreach (var snapshot in runResults)
    {
        Console.WriteLine(snapshot);
    }
}




return;
var x = new FileReaderTest();
x.ConcurrentOperationCount = 100000;
x.MaxIntervalBetweenOperationsMs = 0;

//var t = 8;
//ThreadPool.SetMaxThreads(t,t);
//ThreadPool.SetMinThreads(t,t);

x.ReadFilesSync();
return;








