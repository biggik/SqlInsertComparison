using System;
using System.Data.SqlClient;

namespace SqlInsertComparison
{
    /// <summary>
    /// An action wrapping connection and execution of a single SqlCommand
    /// </summary>
    public static class SqlCommandAction
    {
        public static void Invoke(string connectionString, string command)
        {
            Console.WriteLine(command);
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = command;
            cmd.ExecuteNonQuery();
        } 
    }
}