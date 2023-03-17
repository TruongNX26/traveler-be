using Application.Configurations;
using Application.Configurations.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Shared.Firebase;

// Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.File(path: "logs/log-.txt", rollingInterval: RollingInterval.Month,
        outputTemplate:
        "[{Level:w3}] {Timestamp:dd-MM-yyyy HH:mm:ss.fff zzz}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .CreateLogger();

FirebaseAppInitializer.Init();

// BUILDER
var builder = WebApplication.CreateBuilder(args);
{
    builder.Logging
        .ClearProviders()
        .AddConsole()
        .AddSerilog();

    builder.Services
        .AddCors()
        .AddSwagger()
        .AddEndpointsApiExplorer()
        .AddSwaggerGenNewtonsoftSupport()
        .AddDependencyInjection(builder.Configuration)
        .AddControllers()
        .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }
        );
}

// APPLICATION
var app = builder.Build();
{
    Console.WriteLine(app.Environment.EnvironmentName);
    app
        .UseHttpsRedirection()
        .UseCors(x => x
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin())
        .UseSwagger()
        .UseSwaggerUI()
        .UseMiddleware<JwtMiddleware>();
    // .UseAuthorization();

    app.MapControllers();
    app.Run();
}