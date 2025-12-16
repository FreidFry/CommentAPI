using Comment.Core.Data;
using CommentAPI.Extencions.LoadModules;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<ApiOptions>(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration);

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
    options.AppendTrailingSlash = true;
});
builder.Services.AddPortConfiguration(builder.WebHost);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ["text/plain", "text/css", "application/javascript", "application/json", "image/svg+xml"];
});

builder.Services.AddControllers();

builder.Services.AddAutoMapperModule();
builder.Services.AddDipedencyInjections();


var jwtOptions = new JwtOptions(builder.Configuration);
    builder.Services.AddJwtAuthentication(jwtOptions);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll",
        builder =>
            builder.WithOrigins("https://localhost:54366", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    )
);
builder.Services.AddLogging(builder =>
    builder.AddConsole()
    .AddDebug()
);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
