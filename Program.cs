using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography;

BenchmarkRunner.Run<IOSimulator>();

return;

var x = new IOSimulator();
x.Setup();
x.ConcurrentOperationCount = 100;
x.ReadFilesSync();
return;








