using Comment.Core.Data;
using Comment.Infrastructure.Hubs;
using Comment.Infrastructure.Services.Auth.Login;
using CommentAPI.Extencions.LoadModules;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var runMode = Environment.GetEnvironmentVariable("RUN_MODE") ?? "All";

builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<ApiOptions>(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration);
builder.Services.Configure<KestrelConfig>(builder.Configuration);
var jwtOptions = new JwtOptions(builder.Configuration);
var apiConfig = new ApiOptions(builder.Configuration);

builder.Services.SetupSignalR(apiConfig);
if (runMode == "API" || runMode == "All")
{
    builder.AddSirilogLogger();

    builder.Services.AddControllers().AddJsonOptions(c => c.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    builder.Services.AddJwtAuthentication(jwtOptions);
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true;
        options.MimeTypes = ["text/plain", "text/css", "application/javascript", "application/json", "image/svg+xml"];
    });
}
builder.Services.AddMassTransit(x =>
{
    if (runMode == "Worker" || runMode == "All")
    {
        x.AddConsumers(typeof(LoginConsumer).Assembly);
    }
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(apiConfig.RabbitMqConnect));

        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
    options.AppendTrailingSlash = true;
});
var kestrelConfig = new KestrelConfig(builder.Configuration);
builder.Services.AddPortConfiguration(builder.WebHost, kestrelConfig);



builder.Services.AddRedisCache(apiConfig);

builder.Services.AddAutoMapperModule();
builder.Services.AddDipedencyInjections();



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(apiConfig.DbConnection));


builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll",
        builder =>
            builder.WithOrigins(kestrelConfig.httpDomen, kestrelConfig.httpDomenSecure)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    )
);
builder.Services.AddLogging(builder =>
    builder.AddConsole()
    .AddDebug()
);


var app = builder.Build();


if (runMode == "API" || runMode == "All")
{
    app.UseSerilogRequestLogging();
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}
if (runMode == "API" || runMode == "All")
{
    app.UseCors("AllowAll");

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/notificationHub");
}
app.Run();
