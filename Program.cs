var x = new FileReaderTest();
x.Setup();
x.ConcurrentOperationCount = 1000;
x.MaxIntervalBetweenOperationsMs = 0;

Thread.Sleep(2000);

x.ReadFilesSync();
return;








