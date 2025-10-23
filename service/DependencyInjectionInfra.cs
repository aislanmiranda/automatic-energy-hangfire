using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Spi;
using service.Job;
using service.Queue;
using service.Repository;
using service.Repository.Context;

namespace service
{
	public static class DependencyInjectionInfra
	{
        [Obsolete]
        public static void AddInfrastructure(
            this IServiceCollection services, IConfiguration configuration)
        {
            var HOST = Environment.GetEnvironmentVariable("DBHOST");
            var PORT = Environment.GetEnvironmentVariable("DBPORT");
            var DB = Environment.GetEnvironmentVariable("DBNAME");
            var USER = Environment.GetEnvironmentVariable("DBUSER");
            var PASS = Environment.GetEnvironmentVariable("DBPASS");

            string connectionString = $"Host={HOST};Port={PORT};Database={DB};Username={USER};Password={PASS};Pooling=true;";
            //string connectionQuartz = $"Host=localhost;Port={PORT};Database=quartz;Username=postgres;Password=Quartz@1234;Pooling=true;";

            #if DEBUG
            //connectionString = configuration.GetConnectionString("DefaultConnection")!;
            #endif

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddHangfire(config =>
                config?.UsePostgreSqlStorage(connectionString));

            // Configura Quartz com persistência no PostgreSQL
            services.AddQuartz(q =>
            {
                // Usa o scheduler padrão
                q.UseMicrosoftDependencyInjectionJobFactory();

                // Configura persistência no Postgres
                q.UsePersistentStore(storeOptions =>
                {
                    storeOptions.UsePostgres(connectionString);
                   
                    // 👇 Essas duas linhas são a forma correta de configurar schema e prefixo
                    storeOptions.Properties["quartz.dataSource.default.provider"] = "Npgsql";
                    storeOptions.Properties["quartz.jobStore.driverDelegateType"] = "Quartz.Impl.AdoJobStore.PostgreSQLDelegate, Quartz";
                    //storeOptions.Properties["quartz.jobStore.tablePrefix"] = "qrtz_";
                    storeOptions.Properties["quartz.jobStore.dataSource"] = "default";
                    storeOptions.Properties["quartz.jobStore.useProperties"] = "false";
                    //storeOptions.Properties["quartz.jobStore.schema"] = "quartz"; // 👈 define o schema
                    storeOptions.Properties["quartz.jobStore.tablePrefix"] = "quartz.qrtz_";
                    storeOptions.Properties["quartz.serializer.type"] = "json";
                    storeOptions.Properties["quartz.jobStore.clustered"] = "true";
                    storeOptions.Properties["quartz.dataSource.default.connectionString"] = connectionString;
                });

                q.UseSimpleTypeLoader();
                q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);
            });

            services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);

            services.AddSingleton(provider =>
            {
                var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
                var scheduler = schedulerFactory.GetScheduler().Result;
                scheduler.JobFactory = provider.GetRequiredService<IJobFactory>();
                return scheduler;
            });

            services.AddHangfireServer();
            services.AddSingleton<IRabbitMQService, RabbitMQService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<IEquipamentRepository, EquipamentRepository>();
            services.AddTransient<IMonitoringRepository, MonitoringRepository>();
            services.AddTransient<ITaskRepository, TaskRepository>();
            services.AddTransient<SendMessageJob>();
        }
    }
}

