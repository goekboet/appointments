using System;
using Appointments.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Test.Records
{
    public class ConnectionKey
    {
        public string Host { get; }
        public string Database { get; }
        public string Username { get; }
        public string Password { get; }
        string _v { get; }
        public ConnectionKey(
            string host,
            string database,
            string username,
            string password)
        {
            Host = host;
            Database = database;
            Username = username;
            Password = password;
        } 

        public override string ToString() => 
            $"Host={Host};Database={Database};Username={Username};Password={Password}"; 
    }

    public class TestContextFactory : IDesignTimeDbContextFactory<Pgres>
    {
        public static ConnectionKey DefaultTest { get; } = 
            new ConnectionKey(
                host: "localhost",
                database: "test_appointments",
                username: "appointments",
                password: "nZva7sMNkdF7MBF5"
            );

        public static string GetFresh() => 
            new ConnectionKey(
                host: DefaultTest.Host,
                database: $"{DefaultTest.Database}{Guid.NewGuid().ToString().Substring(0, 4)}",
                username: DefaultTest.Username,
                password: DefaultTest.Password
            ).ToString();

        static ILoggerFactory DebugLogger = new LoggerFactory()
                .AddDebug();

        public Pgres CreateDbContext(string[] args)
        {
            var connectionString = args.Length > 0
                ? args[0]
                : DefaultTest.ToString();

            var optionsBuilder = new DbContextOptionsBuilder<Pgres>();
            optionsBuilder
                .UseLoggerFactory(DebugLogger)
                .UseNpgsql(connectionString);

            return new Pgres(optionsBuilder.Options);
        }
    }
}