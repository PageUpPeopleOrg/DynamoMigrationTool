using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon;
using Amazon.DynamoDBv2;
using Magnum.Extensions;
using PageUp.Formsmith.Database.Helpers;
using PageUp.Formsmith.Database.MigrationFramework;
using PageUp.Formsmith.Database.MigrationFramework.SchemaVersion;

namespace PageUp.Formsmith.Database
{
    public class Program
    {
        private static AmazonDynamoDBClient _client;
        private static Dictionary<MigrationProfile, SortedList<int, IMigration>> _migrations;
        private static SchemaVersion _schemaVersion;

        public static int Main(string[] args)
        {
            try
            {
                _migrations = new Dictionary<MigrationProfile, SortedList<int, IMigration>>();

                // Command line parsing
                var arguments = new Arguments(args);

                var migrationProfile = GetProfile(arguments["Profile"]);

                ConfigureDynamoTableNamespace(migrationProfile.ToString());

                string accessKey = arguments["accesskey"];
                string secretKey = arguments["secretKey"];
                string protocol = arguments["protocol"];
                string host = arguments["host"];
                string port = arguments["port"];

                string serviceUrl = null;
                if (!string.IsNullOrEmpty(protocol) && !string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(port))
                    serviceUrl = string.Concat(protocol, "://", host, ":", port);

                _client = DynamoConnection.CreateConnection(serviceUrl, accessKey, secretKey);

                ConsoleHelpers.Information("Initialising Schema Versions");
                _schemaVersion = new SchemaVersion(_client);
                _schemaVersion.Initialise();
                ConsoleHelpers.Success("Done");

                BuildListOfMigrations(migrationProfile);
                ExecuteMigrations(migrationProfile);

                ConsoleHelpers.Success("Finished migrations.");

                return 0;
            }
            catch (Exception e)
            {
                ConsoleHelpers.Error(e.Message);
                return -1;
            }

        }

        private static void ConfigureDynamoTableNamespace(string profile)
        {
            AWSConfigs.DynamoDBConfig.Context.TableNamePrefix = string.Format("Formsmith.{0}.", profile);
        }

        private static MigrationProfile GetProfile(string profile)
        {
            var profiles = string.Join(",", Enum.GetNames(typeof(MigrationProfile)));
            if (string.IsNullOrEmpty(profile))
            {
                ConsoleHelpers.Error(string.Format("No profile specified. Please specify one of the following: {0}.", profiles));
                throw new ArgumentException();
            }

            try
            {
                return (MigrationProfile)Enum.Parse(typeof(MigrationProfile), profile);
            }
            catch (Exception)
            {
                ConsoleHelpers.Error(string.Format("Invalid profile specified, please specify one of the following: {0}.", profiles));
                throw new ArgumentException();
            }
        }

        private static void ExecuteMigrations(MigrationProfile profile)
        {
            if (!_migrations.ContainsKey(profile))
                throw new ArgumentException(string.Format("Migrations for profile {0} have not been built.", profile));

            if (_migrations.Count == 0)
            {
                ConsoleHelpers.Error("No migrations stored in profile.");
                return;
            }

            ConsoleHelpers.Information(string.Format("Executing migrations for {0} profile.", profile));
            Console.WriteLine();
            foreach (KeyValuePair<int, IMigration> migration in _migrations[profile])
            {
                var scriptName = migration.Value.GetType().FullName;
                if (!_schemaVersion.IsScriptApplied(scriptName))
                {
                    ConsoleHelpers.Information(" " + migration.Key + ". " + scriptName);
                    migration.Value.Execute();
                    _schemaVersion.SetScriptAsApplied(scriptName);
                }
            }
            Console.WriteLine();
            ConsoleHelpers.Success(string.Format("Completed {0} profile.", profile));
        }

        private static void BuildListOfMigrations(MigrationProfile profile)
        {
            var migrations = new SortedList<int, IMigration>();

            ConsoleHelpers.Information(string.Format("Building list of migrations for {0} profile", profile));

            Assembly assembly = typeof(Program).Assembly;

            List<Type> types = assembly
                .GetExportedTypes()
                .Where(type => type.IsConcrete() && type.Is<IMigration>())
                .Where(type => type.HasAttribute<MigrationAttribute>())
                .Where(type => type.GetOneAttribute<MigrationAttribute>().Profile.Equals(profile))
                .ToList();

            foreach (Type type in types)
            {
                var attribute = type.GetOneAttribute<MigrationAttribute>();
                var migration = (IMigration)Activator.CreateInstance(type, _client);

                migrations.Add(attribute.Order, migration);
            }

            _migrations.Add(profile, migrations);

            ConsoleHelpers.Success("Done");
        }


    }
}
