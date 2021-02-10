using System;
using System.Diagnostics;
using System.IO;

namespace SqlInsertComparison
{
    /// <summary>
    /// An action on SqlLocalDb
    /// </summary>
    public static class SqlLocalDbAction 
    {
        private static string fullPath;
        static SqlLocalDbAction()
        {
            const string executable = "SqlLocalDb.exe";
            
            var pathParts = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine).Split(';');
            foreach (var path in pathParts)
            {
                var candidate = Path.Combine(path, executable);
                if (File.Exists(candidate))
                {
                    fullPath = candidate;
                    break;
                }
            }

            if (fullPath == null)
            {
                Console.WriteLine($"{executable} not found on path");
            }
        }
        
        public static void Invoke(string description, string arguments, int waitMS = 10000)
        {
            Console.WriteLine($"{description} [SqlLocalDB {arguments}]");
            var p = Process.Start(fullPath, arguments);
            if (!p.WaitForExit(waitMS))
            {
                throw new Exception($"SqlLocalDB action {arguments} did not complete successfully in the allotted timeframe!");
            }
        } 
    }
}