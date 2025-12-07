using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Spi;
using Refit;
using service.Job;
using service.Provider;
using service.Queue;
using service.Repository;
using service.Repository.Context;
using service.Services;

namespace service;

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
            string ManagerHost = Environment.GetEnvironmentVariable("MANAGERHOST")!;
            string HasSSL = Environment.GetEnvironmentVariable("MANAGERHOSTSSL")!;
            string prefix = HasSSL.Equals("false") ? "http" : "https";
            #if DEBUG
                    //connectionString = configuration.GetConnectionString("DefaultConnection")!;
            #endif

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));

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

            services.AddSingleton<IRabbitMQService, RabbitMQService>();
            services.AddHostedService(sp => (RabbitMQService)sp.GetRequiredService<IRabbitMQService>());

            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<ITaskRepository, TaskRepository>();
            services.AddTransient<IMonitoringRepository, MonitoringRepository>();
            services.AddTransient<IEquipamentRepository, EquipamentRepository>();
            services.AddTransient<SendMessageQueueJob>();

            services.AddMemoryCache();
            services.AddTransient<ITokenProvider, TokenProvider>();
            services.AddTransient<BearerTokenHandler>();

            services.AddRefitClient<IAuthTokenService>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{prefix}://{ManagerHost}"));

            services.AddRefitClient<ISendStatusEquipament>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri($"{prefix}://{ManagerHost}"))
                 .AddHttpMessageHandler<BearerTokenHandler>();
    }
}

public class BearerTokenHandler : DelegatingHandler
{
    private readonly ITokenProvider _scopeFactory;

    public BearerTokenHandler(ITokenProvider scopeFactory)
        => _scopeFactory = scopeFactory;   

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _scopeFactory.GetTokenAsync(cancellationToken);

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}