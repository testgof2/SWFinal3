using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW3_shared
{
  /// <summary>
  /// This exists due to some interfaces being hard to moq
  /// </summary>
  public interface IRepository
  {
    IMongoDatabase ExposeOriginalClient();
    Task<IEnumerable<T>> FindAll<T>();
  };

  public class Repository : IRepository
  {
    private readonly IMongoDatabase _mongoDb;

    public Repository(IMongoDatabase mongoDb)
    {
      _mongoDb = mongoDb;
    }

    public IMongoDatabase ExposeOriginalClient()
    {
      return _mongoDb;
    }

    public async Task<IEnumerable<T>> FindAll<T>()
    {
  //    _mongoDb.DropCollection(typeof(T).Name);
      var collection = _mongoDb.GetCollection<T>(typeof(T).Name);
      
      var res = await collection.Find(Builders<T>.Filter.Empty).ToListAsync();

      return res;
    }
  }
}