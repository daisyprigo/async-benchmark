var x = new FileReaderTest();
x.Setup();
x.ConcurrentOperationCount = 100000;
x.MaxIntervalBetweenOperationsMs = 0;

//var t = 8;
//ThreadPool.SetMaxThreads(t,t);
//ThreadPool.SetMinThreads(t,t);

x.ReadFilesSync();
return;








