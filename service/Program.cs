using AspNetCore.Scalar;
using Hangfire;
using Scalar.AspNetCore;
using service;
using service.Job;
using service.Queue;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHangfireServer();
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddTransient<ITaskService, TaskService>();
builder.Services.AddTransient<HangFireJobService>();

var corsPolicyName = "AllowOrigins";

// Configure a política CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapScalarApiReference();
app.UseScalar(options => {
    options.UseTheme(Theme.Default);
    options.RoutePrefix = "api-docs";
});

app.UseCors(corsPolicyName);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard();

app.Run();