using System;
using System.Collections.Generic;
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
        public static IEnumerable<string> GetRandomTestFileSample(string dirPath, int fileCount)
        {
            var testFileList = GetTestFileList(dirPath);
            var testFileSet = new List<string>(fileCount);
            
            var rand = new Random();
            for (int i = 0; i < fileCount; i++)
            {
                testFileSet.Add(testFileList[rand.Next(testFileList.Length)]);
            }
            return testFileSet;
        }
        /// <summary>
        /// Gets a full list of files from some folder we want to use as source of test files to read
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private static string[] GetTestFileList(string folderPath)
        {
            return Directory.GetFiles(folderPath);
        }
    }
}
