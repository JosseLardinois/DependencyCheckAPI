using BackgroundTasks.Worker;
using DependencyCheckAPI.DAL;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Repositories;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDependencyScanRepository, DependencyScanRepository>();
builder.Services.AddSingleton<IAzureBlobStorage, AzureBlobStorage>();
builder.Services.AddSingleton<IExtractJson, ExtractJsonRepository>();
builder.Services.AddSingleton<IAzureFileRepository, AzureFileRepository>();
builder.Services.AddSingleton<ISQLResultsStorage, SQLResultsStorage>();
builder.Services.AddSingleton<ISQLResultsRepository, SQLResultsRepository>();
builder.Services.AddHostedService<Worker>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
