using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace SqlInsertComparison
{
    class Program
    {
        const string InstanceName = "SqlInsertComparison";
        const string DatabaseName = "BulkInsertSample";
        
        static string ConnectionStringToMaster = $@"Data Source=(LocalDB)\{InstanceName};initial catalog=master;Integrated Security=true;";
        static string ConnectionStringToDatabase = $@"Data Source=(LocalDB)\{InstanceName};initial catalog={DatabaseName};Integrated Security=true;";

        static Stopwatch sw = new Stopwatch();
        static Random random = new Random();

        static void Main(string[] args)
        {
            var sd = new StackDisposable();
            try
            {
                // Create instance and database
                sd.Push(CreateInstanceAndDatabase());

                // Get the row count from the command line, default to 1000
                int total = args.Length > 0 && int.TryParse(args[0], out int specifiedTotal) ? specifiedTotal : 1000;

                // Run each of the three insert "methods"
                var iterativeTime = Run("IterativeInsert", () => IterativeInsert(total));
                var batchTime = Run("BatchInsert", () => BatchInsert(total));
                var bulkTime = Run("BulkInsert", () => BulkInsert(total));

                // Show the result of each
                ShowResults(("IterativeInsert", iterativeTime), ("BatchInsert", batchTime), ("BulkInsert",  bulkTime));
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
            }
        
            // Using disposable to clean up (drop instance, and database)
            sd.Dispose();
        }

        private static TimeSpan Run(string actionName, Action action)
        {
            sw.Reset();
            sw.Start();
            action.Invoke();
            sw.Stop();
            Console.WriteLine($"{actionName} insert done in {sw.Elapsed}");
            return sw.Elapsed;
        }

        private static IDisposable CreateInstanceAndDatabase()
        {
            var sd = new StackDisposable();
            try
            {
                SqlLocalDbAction.Invoke("Creating new LocalDB instance", $"c {InstanceName}"); // Create instance
                sd.Push(new ActionDisposable(() => SqlLocalDbAction.Invoke("Deleting LocalDB instance", $"d {InstanceName}"))); // Delete instance

                SqlLocalDbAction.Invoke("Starting LocalDB instance", $"s {InstanceName}"); // Start instance
                sd.Push(new ActionDisposable(() => SqlLocalDbAction.Invoke("Stopping LocalDB instance", $"p {InstanceName} -k"))); // Stop instance
                
                SqlCommandAction.Invoke(ConnectionStringToMaster, $"CREATE DATABASE {DatabaseName}");
                sd.Push(new ActionDisposable(() => SqlCommandAction.Invoke(ConnectionStringToMaster, $"ALTER DATABASE {DatabaseName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE {DatabaseName}")));
            }
            
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sd.Dispose();
                return null;
            }

            return sd;
        }

        private static void CreateTable(SqlConnection conn, string tableName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"create table {tableName} (
                                        id int identity(1,1),
                                        Name nvarchar(50),
                                        Age int)
                                   ";
            cmd.ExecuteNonQuery();
        }

        private static string NewValueClause()
        {
            int age = random.Next(25, 70);
            return $"('Someone {age} year old', {age})";
        }

        private static void IterativeInsert(int rowsToInsert)
        {
            using var conn = new SqlConnection(ConnectionStringToDatabase);
            conn.Open();

            CreateTable(conn, "IterativeInsert");

            Console.WriteLine($"Writing {rowsToInsert} records iteratively");
            using var cmdInsert = conn.CreateCommand();
            for (int i = 0; i < rowsToInsert; i++)
            {
                cmdInsert.CommandText = $"insert into IterativeInsert(Name, Age) values{NewValueClause()}";
                cmdInsert.ExecuteNonQuery();
            }
        }
        
        private static void BatchInsert(int rowsToInsert)
        {
            const int batchSize = 1000; // Shouldn't be higher
            
            using var conn = new SqlConnection(ConnectionStringToDatabase);
            conn.Open();

            CreateTable(conn, "BatchInsert");

            using var cmdInsert = conn.CreateCommand();

            Console.WriteLine($"Writing {rowsToInsert} records in batches of {Math.Min(batchSize, rowsToInsert)}");
            while (rowsToInsert > 0)
            {
                var batch = new List<string>();
                var currentBatch = Math.Min(batchSize, rowsToInsert);
                while (currentBatch-- > 0)
                {
                    batch.Add(NewValueClause());
                }

                rowsToInsert -= batch.Count;
                cmdInsert.CommandText = $"insert into BatchInsert(Name, Age) values {string.Join(",", batch)}";
                cmdInsert.ExecuteNonQuery();
            }
        }
        private static void BulkInsert(int rowsToInsert)
        {
            using var conn = new SqlConnection(ConnectionStringToDatabase);
            conn.Open();

            CreateTable(conn, "BulkInsert");
            
            Console.WriteLine($"Bulk inserting {rowsToInsert} records");

            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Age", typeof(int));

            for (int i = 0; i < rowsToInsert; i++)
            {
                int age = random.Next(25, 70);
                table.Rows.Add($"Someone {age} year old", age);
            }

            using var bulk = new SqlBulkCopy(conn);
            bulk.DestinationTableName = "BulkInsert";
            bulk.WriteToServer(table);
        }
        
        private static void ShowResults(params (string table, TimeSpan time)[] results)
        {
            var maxTime = (from result in results
                           select result.time).Max().TotalMilliseconds;
            
            using var conn = new SqlConnection(ConnectionStringToDatabase);
            conn.Open();

            foreach (var result in results)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"select count(*) from {result.table}";
                var count = Convert.ToInt32(cmd.ExecuteScalar());

                var p = (100 * result.time.TotalMilliseconds / maxTime).ToString("n2").PadLeft(6);
                var s = $"'{result.table}'".PadRight(30);
                Console.WriteLine($"Table {s} has {count} rows inserted in {result.time} [{p}% of max]");
            }
        }

    }
}