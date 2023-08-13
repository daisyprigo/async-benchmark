var x = new FileReaderTest();
x.Setup();
x.ConcurrentOperationCount = 100000;
x.MaxIntervalBetweenOperationsMs = 0;

Thread.Sleep(2000);

x.ReadFilesASync();
return;








