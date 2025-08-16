using System;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Hosting;

namespace service
{
	public static class DependencyInjectionInfra
	{
        public static void AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
        {
            var HOST = Environment.GetEnvironmentVariable("DBHOST");
            var PORT = Environment.GetEnvironmentVariable("DBPORT");
            var DB = Environment.GetEnvironmentVariable("DBNAME");
            var USER = Environment.GetEnvironmentVariable("DBUSER");
            var PASS = Environment.GetEnvironmentVariable("DBPASS");

            string connectionString = $"Host={HOST};Port={PORT};Database={DB};Username={USER};Password={PASS};Pooling=true;";

            #if DEBUG
            connectionString = configuration.GetConnectionString("DefaultConnection")!;
            #endif
            Console.WriteLine("Connection string: " + connectionString);

            services.AddHangfire(config =>
                config?.UsePostgreSqlStorage(connectionString));
        }
    }
}

