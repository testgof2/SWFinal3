using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using RabbitMQ.Client;
using System.Threading.Channels;
using System.Text;
using SW3_shared.Models;

namespace SW3_DataCollector.Services
{
  public interface IDataIntakeService
  {
    Task IntakeData(IncomingData dto);
  }

  public class DataIntakeService : IDataIntakeService
  {
    private readonly IMongoDatabase _mongoDb;
    private readonly IModel _channel;



    public DataIntakeService(IMongoDatabase db, IModel channel)
    {
      _mongoDb = db;
      _channel = channel;
    }

    public async Task IntakeData(IncomingData dto)
    {
      await _mongoDb.GetCollection<IncomingData>("DataIntake").InsertOneAsync(dto);

      var serialized = JsonSerializer.Serialize(dto);
      var body = Encoding.UTF8.GetBytes(serialized);

      _channel.BasicPublish(string.Empty,
        "DataExchange",
        false,
        null,
        body);

    }
  }
}