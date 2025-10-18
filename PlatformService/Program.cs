using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PlatformService.Data;
using PlatformService.Profiles;
using PlatformService.SyncDataServices;
using PlatformService.SyncDataServices.Http;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();

if (builder.Environment.IsProduction())
{
    Console.WriteLine($" --- UseSqlServer {builder.Configuration.GetConnectionString("PlatformsConn")}  ---- ");
    builder.Services
        .AddDbContext<AppDbContext>(opt 
        => opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
}
else
{
    Console.WriteLine($" ---- UseInMemoryDatabase --- ");
    builder.Services
        .AddDbContext<AppDbContext>(opt => 
        opt.UseInMemoryDatabase("InMem"));
}


builder.Services.AddAutoMapper(typeof(PlatformsProfile));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
});
builder.Services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();
Console.WriteLine($"--> CommandService Endpoint {builder.Configuration["CommandService"]}");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
PrepDb.PrepPopulation(app, app.Environment.IsProduction());
app.Run();
