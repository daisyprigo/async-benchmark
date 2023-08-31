using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Async_Benchmark
{
    internal record CounterSnapshot(bool IsAsync, DateTime TimeStamp, int TimePoint, int JobsNotStarted, int JobsAtStage1, int JobsAtStage2, int JobsAtStage3, int JobsCompleted, 
        int ThreadCount, 
        long CompletedWorkItemsInTP,
        long PendingWorkItemsInTP);

    internal class ParallelRequestSimulator
    {
        public int MaxParallelOperationCount;

        public int RequestsPerSec = 100;

        private int fileCounter;

        private int jobsNotStarted = 0;
        private int jobsAtStage1 = 0;
        private int jobsAtStage2 = 0;
        private int jobsAtStage3 = 0;
        private int jobsCompleted = 0;
        private List<CounterSnapshot> snapshots = new List<CounterSnapshot>();

        bool stopLoggingCounters = false;

        private void LogCounters(bool isAsync)
        {
            var timePoint = 0;
            while (!stopLoggingCounters)
            {
                CreateSnapshot(isAsync);
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
            }
            CreateSnapshot(isAsync);

            void CreateSnapshot(bool isAsync)
            {
                var snapshot = new CounterSnapshot(
                                    IsAsync: isAsync,
                                    TimeStamp: DateTime.Now,
                                    TimePoint: timePoint++,
                                    JobsNotStarted: this.jobsNotStarted,
                                    JobsAtStage1: this.jobsAtStage1,
                                    JobsAtStage2: this.jobsAtStage2,
                                    JobsAtStage3: this.jobsAtStage3,
                                    JobsCompleted: this.jobsCompleted,
                                    ThreadCount: ThreadPool.ThreadCount,
                                    CompletedWorkItemsInTP: ThreadPool.CompletedWorkItemCount,
                                    PendingWorkItemsInTP: ThreadPool.PendingWorkItemCount
                                    );

                snapshots.Add(snapshot);
            }
        }

        public List<CounterSnapshot> ExecuteTest(bool useAsyncIO, IEnumerable<string> filesToLoad)
        {
            var rand = new Random();
           
            //var filesToLoad = Helpers.GetRandomTestFileSample(@"m:\LightroomArchive\Lightroom CC\", TotalOperationCount, true);
            // we store all the tasks that we launched here
            var runningTasks = new List<Task>();

            var perfSw = Stopwatch.StartNew();
            var requestBurstSw = Stopwatch.StartNew();
            int requestsBurstCounter = 0;
            var burstDuration = TimeSpan.FromMilliseconds(100);
            int maxRequestInBurst = (int)(RequestsPerSec / (1 / burstDuration.TotalSeconds));

            Thread counterLoggingThread = new Thread(() => this.LogCounters(useAsyncIO));
            counterLoggingThread.Name = "Counter monitoring thread";
            counterLoggingThread.Start();

            foreach (var file in filesToLoad)
            {
                // here we are only "launching" the read operation, but we are not waiting for it to complete
                // how the operation will be executed (sync vs async) will be determined by the implementation that is passed to us as "fileRead"
                Interlocked.Increment(ref jobsNotStarted);
                var task = Task.Run(async () => await ReadFileContent(file, useAsyncIO));
                //var task = Task.Run(async () => await SimulateIO(async));
                // ignore for now
                task.ConfigureAwait(false);
                runningTasks.Add(task);
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

            // stopping the counter thread and waiting for it
            this.stopLoggingCounters = true;
            counterLoggingThread.Join();

            Console.WriteLine($"Finished waiting for all tasks to complete, took: {perfSw.Elapsed.TotalSeconds} seconds.");
            return snapshots;
        }

        private async Task ReadFileContent(string path, bool async)
        {
            var sw = Stopwatch.StartNew();
            byte[] bytes = Array.Empty<byte>();

            // Stage 1: simulate some time spent processing the request before IO
            Interlocked.Decrement(ref jobsNotStarted);
            Interlocked.Increment(ref jobsAtStage1);
            Helpers.SimulateComputation(TimeSpan.FromMilliseconds(50));

            // Stage 2: do the IO
            Interlocked.Decrement(ref jobsAtStage1);
            Interlocked.Increment(ref jobsAtStage2);
            if (async)
                // this does not block the current thread, because it uses an async method
                bytes = await File.ReadAllBytesAsync(path).ConfigureAwait(false);
            else
                // this DOES block the current thread, because we use a sync method
                bytes = File.ReadAllBytes(path);

            //var delay = TimeSpan.FromMilliseconds(500);
            //if (async)
            //    // this does not block the thread
            //    await Task.Delay(delay);
            //else
            //    // this blocks the thread
            //    Thread.Sleep(delay);

            // Stage 3: simulate some time done post processing on the request
            Interlocked.Decrement(ref jobsAtStage2);
            Interlocked.Increment(ref jobsAtStage3);
            Helpers.SimulateComputation(TimeSpan.FromMilliseconds(50));

            var count = Interlocked.Increment(ref fileCounter);
            Console.WriteLine($"{count} - file read: {path}, bytes: {bytes.Length}, time-ms: {sw.ElapsedMilliseconds}");

            Interlocked.Decrement(ref jobsAtStage3);
            Interlocked.Increment(ref jobsCompleted);
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
