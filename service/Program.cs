using AspNetCore.Scalar;
using Scalar.AspNetCore;
using service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);

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
app.UseMiddleware<TokenCaptureMiddleware>();
app.UseCors(corsPolicyName);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();