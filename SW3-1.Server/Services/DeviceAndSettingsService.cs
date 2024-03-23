using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using SW3_1.Server.Models;
using SW3_shared;

namespace SW3_1.Server.Services
{
  public interface IDeviceAndSettingsService
  {
    Task<IEnumerable<DeviceAndSettings>> GetDeviceInfoAsync();
    Task CreateNewDevice(DeviceAndSettings dto);
    Task EditDevice(DeviceAndSettings dto);
    Task DeleteDevice(Guid deviceId);
  }

  public class DeviceAndSettingsService : IDeviceAndSettingsService
  {
    private readonly IRepository _repository;


    public DeviceAndSettingsService(IRepository repository)
    {
      _repository = repository;
    }

    public async Task CreateNewDevice(DeviceAndSettings dto)
    {
      var collection = _repository.ExposeOriginalClient().GetCollection<DeviceAndSettings>(nameof(DeviceAndSettings));
      await collection.InsertOneAsync(dto);
    }

    public async Task DeleteDevice(Guid deviceId)
    {
      var collection = _repository.ExposeOriginalClient().GetCollection<DeviceAndSettings>(nameof(DeviceAndSettings));
      var filter = Builders<DeviceAndSettings>.Filter.Eq(p => p.Id, deviceId);
      await collection.DeleteOneAsync(filter);
    }

    public async Task EditDevice(DeviceAndSettings dto)
    {
      var collection = _repository.ExposeOriginalClient().GetCollection<DeviceAndSettings>(nameof(DeviceAndSettings));
      var filter = Builders<DeviceAndSettings>.Filter.Eq(p => p.Id, dto.Id);
      var result = await collection.ReplaceOneAsync(filter, dto);
    }

    public async Task<IEnumerable<DeviceAndSettings>> GetDeviceInfoAsync()
    {
      var res = await _repository.FindAll<DeviceAndSettings>();
      return res;
    }
  }
}