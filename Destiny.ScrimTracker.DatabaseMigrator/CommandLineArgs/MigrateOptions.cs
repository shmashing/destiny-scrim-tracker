using CommandLine;

namespace Destiny.ScrimTracker.DatabaseMigrator.CommandLineArgs
{
    public class MigrateOptions
    {
        [Option('s', "server", Required = true, HelpText = "Name of the db server endpoint")]
        public string Server { get; set; }
        [Option('d', "datebase", Required = true, HelpText = "Name of the database")]
        public string Database { get; set; }
        [Option('p', "port", Default = "5432", HelpText = "Database port")]
        public string Port { get; set; }
        [Option('u', "user", Required = true, HelpText = "Database user id")]
        public string User { get; set; }
        [Option('w', "password", Required = true, HelpText = "Password for database user")]
        public string Password { get; set; }
        [Option('l', "script-location", Required = true, HelpText = "Relative location of the SQL script to run")]
        public string ScriptLocation { get; set; }
        [Option('v', "verbose", Required = false, Default = false, HelpText = "Enables detailed logging messages while running SQL scripts")]
        public bool LogVerbose { get; set; }
    }
}