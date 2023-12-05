using BackgroundTasks.Worker;
using DependencyCheckAPI.DAL;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Service
    ;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDependencyScanService, DependencyScanService>();
builder.Services.AddSingleton<IReportRepository, ReportRepository>();
builder.Services.AddSingleton<IExtractJsonService, ExtractJsonService>();
builder.Services.AddSingleton<ISQLResultsStorageRepository, SQLResultsStorageRepository>();
builder.Services.AddSingleton<ISQLResultsService, SQLResultsService>();
builder.Services.AddHostedService<Worker>();
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
