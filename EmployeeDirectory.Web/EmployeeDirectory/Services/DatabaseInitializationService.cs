using Microsoft.Extensions.Configuration;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace EmployeeDirectory.Services
{
    public class DatabaseInitializationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseInitializationService> _logger;

        public DatabaseInitializationService(
            IConfiguration configuration,
            ILogger<DatabaseInitializationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Connection string 'DefaultConnection' is not configured");
                return;
            }

            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var databaseName = builder.Database;
            
            if (string.IsNullOrEmpty(databaseName))
            {
                _logger.LogError("Database name is not specified in connection string");
                return;
            }

            builder.Database = "postgres";

            try
            {
                using var connection = new NpgsqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                var checkDbQuery = $@"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
                using var checkCommand = new NpgsqlCommand(checkDbQuery, connection);
                var dbExists = await checkCommand.ExecuteScalarAsync();

                if (dbExists == null)
                {
                    _logger.LogInformation("Creating database {DatabaseName}", databaseName);
                    
                    var createDbQuery = $@"CREATE DATABASE ""{databaseName}""";
                    using var createCommand = new NpgsqlCommand(createDbQuery, connection);
                    createCommand.CommandTimeout = 300;
                    await createCommand.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation("Database {DatabaseName} created successfully", databaseName);
                }
                else
                {
                    _logger.LogInformation("Database {DatabaseName} already exists", databaseName);
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "42P04")
            {
                _logger.LogInformation("Database {DatabaseName} already exists (checked via exception)", databaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking/creating database {DatabaseName}", databaseName);
                throw;
            }
        }
    }
}

