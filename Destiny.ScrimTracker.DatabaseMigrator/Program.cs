using System;
using CommandLine;
using DbUp;
using Destiny.ScrimTracker.DatabaseMigrator.CommandLineArgs;

namespace Destiny.ScrimTracker.DatabaseMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<MigrateOptions>(args)
                .WithParsed(RunDatabaseMigration);
        }
        
        private static void RunDatabaseMigration(MigrateOptions migrateOptions)
        {
            var server = migrateOptions.Server;
            var database = migrateOptions.Database;
            var port = migrateOptions.Port;
            var user = migrateOptions.User;
            var password = migrateOptions.Password;
            var scriptLocation = migrateOptions.ScriptLocation;

            var logVerbose = migrateOptions.LogVerbose;
            
            var connectionString = $"Server={server};Database={database};Port={port};User Id={user};Password={password}";

            RunMigration(connectionString, scriptLocation, logVerbose);
        }

        private static void RunMigration(string connectionString, string scriptLocation, bool logVerbose)
        {
            try
            {
                Console.Write("Starting database migrations... ");
                var engineBuilder = DeployChanges.To
                    .PostgresqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(scriptLocation);

                if (logVerbose)
                {
                    engineBuilder.LogToConsole();
                    engineBuilder.LogScriptOutput();
                }
                else
                {
                    engineBuilder.LogToNowhere();
                }

                var upgrader = engineBuilder.Build();

                var result = upgrader.PerformUpgrade();

                if (!result.Successful)
                {
                    Console.Write("failed.\n");
                    Console.WriteLine(result.Error);
                    return;
                }

                Console.Write("done.\n");
            }
            catch (Exception ex)
            {
                Console.Write("failed.\n");
                Console.WriteLine(ex.Message);
            }
        }
    }
}