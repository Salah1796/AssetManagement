using CommandsService.AsyncDataServices;
using CommandsService.Data;
using CommandsService.EventProcessing;
using CommandsService.Profiles;
using CommandsService.SyncDataServices;
using CommandsService.SyncDataServices.Grpc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(typeof(CommandsProfile));

builder.Services
      .AddDbContext<AppDbContext>(opt =>
      opt.UseInMemoryDatabase("InMem"));

builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CommandsService", Version = "v1" });
});

builder.Services.AddSingleton<IEventProcessor, EventProcessor>();
builder.Services.AddHostedService<RabbitMQSubscriber>();
builder.Services.AddScoped<IPlatformDataClient, GrpcDataClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CommandsService v1"));
}

PrepDb.PrepPopulation(app);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
