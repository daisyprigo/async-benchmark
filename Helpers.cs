using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Async_Benchmark
{
    internal static class Helpers
    {
        /// <summary>
        /// Gets a small set of random files to use for a test run
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetRandomTestFileSample(string dirPath, int fileCount, bool subDirs)
        {
            var testFileList = Directory.GetFiles(dirPath, "*.*", subDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            var testFileSet = new List<string>(fileCount);
            
            var rand = new Random();
            for (int i = 0; i < fileCount; i++)
            {
                testFileSet.Add(testFileList[rand.Next(testFileList.Length)]);
            }
            return testFileSet;
        }


        public static void SimulateComputation(TimeSpan duration)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < duration) { }
        }
    }
}
