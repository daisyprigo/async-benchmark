using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Async_Benchmark
{
    internal class ParallelRequestSimulator
    {
        public int MaxParallelOperationCount;

        public int TotalOperationCount;

        public int MaxIntervalBetweenOperationsMs;

        public int RequestsPerSec = 100;

        private int fileCounter;

        public void ExecuteTest(bool async)
        {
            var rand = new Random();
            // a random list of files that we will use for the test
            //var filesToLoad = Helpers.GetRandomTestFileSample(@"c:\windows\system32", TotalOperationCount);
            var filesToLoad = Helpers.GetRandomTestFileSample(@"m:\LightroomArchive\Lightroom CC\", TotalOperationCount, true);
            // we store all the tasks that we launched here
            var runningTasks = new List<Task>(TotalOperationCount);

            var perfSw = Stopwatch.StartNew();
            var requestBurstSw = Stopwatch.StartNew();
            int requestsBurstCounter = 0;
            var burstDuration = TimeSpan.FromMilliseconds(100);
            int maxRequestInBurst = (int)(RequestsPerSec / (1 / burstDuration.TotalSeconds));

            foreach (var file in filesToLoad)
            {
                // here we are only "launching" the read operation, but we are not waiting for it to complete
                // how the operation will be executed (sync vs async) will be determined by the implementation that is passed to us as "fileRead"
                var task = Task.Run(async () => await ReadFileContent(file, async));
                //var task = Task.Run(async () => await SimulateIO(async));
                // ignore for now
                task.ConfigureAwait(false);
                runningTasks.Add(task);
                // Wait a little bit between read operations, to simulate a distribution of incoming requests over time
                if (MaxIntervalBetweenOperationsMs > 0)
                {
                    Thread.Sleep(rand.Next(MaxIntervalBetweenOperationsMs));
                }
                if (++requestsBurstCounter >= maxRequestInBurst) 
                {
                    // rest the burst
                    requestsBurstCounter = 0;
                    // pause the main thread until the full second has passed since the start
                    var sleepTime = burstDuration - requestBurstSw.Elapsed;
                    if (sleepTime > TimeSpan.Zero)
                    {
                        Thread.Sleep(sleepTime);
                    }
                    requestBurstSw.Restart();
                }

            }
            Console.WriteLine($"Finished launching all operations, took: {perfSw.Elapsed.TotalSeconds} seconds.");
            for (int i = 0; i < 1; i++)
            {
                Console.Beep();
            }
            

            perfSw.Restart();
            // wait for all read operations to complete
            Task.WaitAll(runningTasks.ToArray());
            Console.WriteLine($"Finished waiting for all tasks to complete, took: {perfSw.Elapsed.TotalSeconds} seconds.");
        }

        private async Task ReadFileContent(string path, bool async)
        {
            var sw = Stopwatch.StartNew();
            byte[] bytes;

            // simulate some time spent processing the request before IO
            Helpers.SimulateComputation(TimeSpan.FromMilliseconds(50));

            // do the IO
            if (async)
                bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            else
                bytes = File.ReadAllBytes(path);

            // simulate some time done post processing on the request
            Helpers.SimulateComputation(TimeSpan.FromMilliseconds(50));

            var count = Interlocked.Increment(ref fileCounter);
            Console.WriteLine($"{count} - file read: {path}, bytes: {bytes.Length}, time-ms: {sw.ElapsedMilliseconds}");
            return;
        }

        private async Task SimulateIO(bool async)
        {
            var delay = TimeSpan.FromSeconds(10);
            var sw = Stopwatch.StartNew();
            if (async)
                await Task.Delay(delay);
            else
                Thread.Sleep(delay);

            var count = Interlocked.Increment(ref fileCounter);
            Console.WriteLine($"{count} - simulated operation complete: {sw.ElapsedMilliseconds}");
            return;
        }
    }
}
