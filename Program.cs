
using Async_Benchmark;

var mt = 4;

ThreadPool.SetMaxThreads(mt, mt);

var t = new ParallelRequestSimulator();
t.TotalOperationCount = 1000;
t.ExecuteTest(true);


return;
var x = new FileReaderTest();
x.ConcurrentOperationCount = 100000;
x.MaxIntervalBetweenOperationsMs = 0;

//var t = 8;
//ThreadPool.SetMaxThreads(t,t);
//ThreadPool.SetMinThreads(t,t);

x.ReadFilesSync();
return;








