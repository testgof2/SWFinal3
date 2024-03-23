using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SW3_1.Server.Services;
using SW3_shared;
using System.Text.Json;
using System.Text;
using SW3_shared.Models;
using Microsoft.Extensions.Configuration;


namespace SW3_1.Server
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.
      var configuration = builder.Configuration;
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
        //  HostName = rabbitSettings["HostName"],
        //  Port = int.Parse(rabbitSettings["Port"]),
        // UserName = rabbitSettings["UserName"],
        // Password = rabbitSettings["Password"],
        //  VirtualHost = rabbitSettings["VirtualHost"]
      };
      using var connection = factory.CreateConnection();
      using var channel = connection.CreateModel();

      var isDeclared = channel.QueueDeclare(queue: "DataExchange",
        durable: false,
        exclusive: false,
        autoDelete: false,
        arguments: null);


      builder.Services.AddSingleton(channel);
      builder.Services.AddSingleton(database);
      builder.Services.AddScoped<IAnalyzedDataService, AnalyzedDataService>();
      builder.Services.AddScoped<IDeviceAndSettingsService, DeviceAndSettingsService>();
      builder.Services.AddScoped<IRepository, Repository>();


      var app = builder.Build();


      var consumer = new EventingBasicConsumer(channel);
      consumer.Received += async (model, ea) =>
      {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var data = JsonSerializer.Deserialize<IncomingData>(message);
        using (var scope = app.Services.CreateScope())
        {
          var service = scope.ServiceProvider.GetService<IAnalyzedDataService>();
          await service.HandleIncomingData(data);
        }
      };

      channel.BasicConsume(queue: "DataExchange",
        autoAck: true,
        consumer: consumer);

      app.UseDefaultFiles();
      app.UseStaticFiles();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
        app.UseSwagger();
        app.UseSwaggerUI();
      }

      app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());


      app.UseHttpsRedirection();

      app.UseAuthorization();

      app.MapControllers();
      app.UseRouting();



      app.Run();
    }
  }
}