using MongoDB.Driver;
using RabbitMQ.Client;
using SW3_DataCollector.Services;
using SW3_shared;

namespace SW3_DataCollector
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      var configuration = builder.Configuration;
      // Add services to the container.

      builder.Services.AddControllers();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();


      var mongoSettings = configuration.GetSection("DataSources").GetSection("MongoDB");
      var client = new MongoClient(mongoSettings["ConnectionString"]);
      var database = client.GetDatabase(mongoSettings["DatabaseName"]);

      // Configure RabbitMQ
      var rabbitSettings = configuration.GetSection("DataSources").GetSection("RabbitMQ");
      var factory = new ConnectionFactory
      {
        Uri = new Uri(rabbitSettings["externalConnString"])
        // HostName = rabbitSettings["HostName"],
        // Port = int.Parse(rabbitSettings["Port"]),
        // UserName = rabbitSettings["UserName"],
        //  Password = rabbitSettings["Password"],
        // VirtualHost = rabbitSettings["VirtualHost"]
      };
      using var connection = factory.CreateConnection();
      using var channel = connection.CreateModel(); 
       
      builder.Services.AddSingleton(channel);
      builder.Services.AddSingleton(database);
      builder.Services.AddScoped<IDataIntakeService, DataIntakeService>();
      builder.Services.AddScoped<IRepository, Repository>();

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseHttpsRedirection();

      app.UseAuthorization();

      app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
      app.MapControllers();
      app.UseRouting();

 

      app.Run();
    }
  }
}