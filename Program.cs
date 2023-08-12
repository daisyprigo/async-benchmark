using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;

//BenchmarkRunner.Run<IOSimulator>();

//return;

var x = new IOSimulator();
x.Setup();
x.ConcurrentOperationCount = 10000;
x.MaxIntervalBetweenOperationsMs = 0;
x.MinOperationDurationMs = 5000;
x.SimulatedProcessingTimeInIterations = 1000;

Thread.Sleep(2000);


x.SimulateSyncIOOperations();
return;








