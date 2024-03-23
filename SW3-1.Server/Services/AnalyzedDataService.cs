using MongoDB.Driver;
using SW3_1.Server.Models;
using SW3_shared;
using SW3_shared.Models;
using static MongoDB.Driver.WriteConcern;

namespace SW3_1.Server.Services
{
  public interface IAnalyzedDataService
  {
    Task<IEnumerable<AnalyzedData>> AnalyzeData(DeviceAndSettings deviceAndSettings);


    Task HandleIncomingData(IncomingData data);
  }

  public class AnalyzedDataService : IAnalyzedDataService
  {
    private readonly IRepository _repository;
    private readonly int _numberOfKeptEntries = 10;

    public AnalyzedDataService(IRepository repository)
    {
      _repository = repository;
    }

    public async Task<IEnumerable<AnalyzedData>> AnalyzeData(DeviceAndSettings deviceAndSettings)
    {


      var analyzedDataList = new List<AnalyzedData>();

      for (int i = 0; i < deviceAndSettings.DataAnnotations.Count(); i++)
      {

        var dataForType = deviceAndSettings.LatestData.Select(x => x.Values.ElementAt(i));

        var analyzedData = new AnalyzedData
        {
          Average = dataForType.Average(),
          Median = GetMedian(dataForType.ToList()),
        };
        analyzedDataList.Add(analyzedData);
      }


      return analyzedDataList;
    }


    public async Task HandleIncomingData(IncomingData data)
    {
      var collection = _repository.ExposeOriginalClient().GetCollection<DeviceAndSettings>(nameof(DeviceAndSettings));
      var device = (await _repository.FindAll<DeviceAndSettings>()).First(x => x.Id == data.DeviceId);


      // Add the new incoming data to the device's latest data
      var updatedData = device.LatestData.Append(
         new() { Values = data.Data, CreatedAt = DateTime.Now } );

      // Ensure that only the 10 latest entries are kept
      if (updatedData.Count() > 10)
      {
        updatedData = updatedData.Skip(updatedData.Count() - _numberOfKeptEntries).ToList();
      }

      device.LatestData = updatedData;

      // Update the device in the database
      var filter = Builders<DeviceAndSettings>.Filter.Eq("Id", device.Id);
      var update = Builders<DeviceAndSettings>.Update.Set("LatestData", device.LatestData);
      await collection.UpdateOneAsync(filter, update);
    }

    private float GetMedian(List<float> values)
    {
      var sortedValues = values.OrderBy(v => v).ToList();
      int size = sortedValues.Count;
      int mid = size / 2;

      if (size % 2 == 0)
      {
        return (sortedValues[mid - 1] + sortedValues[mid]) / 2;
      }
      else
      {
        return sortedValues[mid];
      }
    }
  }
}