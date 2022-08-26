using WebApi;
using WebApi.TorchUtilities.Models;
using WebApi.TorchUtilities.Services;

var builder = WebApplication.CreateBuilder(args);

App.Database = new(builder.Configuration["ConnectionStrings:SqlServer"] + ";MultipleActiveResultSets=true");

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
