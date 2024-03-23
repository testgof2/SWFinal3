using System.Text;
using System.Text.Json;
using AutoFixture;
using AutoFixture.AutoMoq;
using MongoDB.Driver;
using Moq;
using RabbitMQ.Client;
using SW3_DataCollector.Services;
using SW3_shared.Models;


namespace DataCollectorTests.UnitTests
{
  [TestClass]
  [TestCategory("Unit")]
  public class DataIntakeServiceTests
  {
    private IFixture _fixture;
    private Mock<IMongoDatabase> _mockMongoDb;
    private Mock<IModel> _mockChannel;
    private Mock<IMongoCollection<IncomingData>> _mockCollection;
    bool _isPublished = false;


    [TestInitialize]
    public void Initialize()
    {
      _fixture = new Fixture().Customize(new AutoMoqCustomization());
      _mockMongoDb = _fixture.Freeze<Mock<IMongoDatabase>>();
      _mockChannel = _fixture.Freeze<Mock<IModel>>();

      _isPublished = false;
      _mockChannel.Setup(x => x.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
        It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>())).Callback(() => { _isPublished = true; });
      _mockCollection = new Mock<IMongoCollection<IncomingData>>();

      _mockCollection.Setup(x => x.InsertOneAsync(It.IsAny<IncomingData>(), null,It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

      // Setup MongoDB collection
      _mockMongoDb.Setup(db => db.GetCollection<IncomingData>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
        .Returns(_mockCollection.Object);
    }

    [TestMethod]
    public async Task IntakeData_SavesDataAndSendsMessage()
    {
      // Arrange
      var service = new DataIntakeService(_mockMongoDb.Object, _mockChannel.Object);
      var incomingData = _fixture.Create<IncomingData>();
      var serializedData = JsonSerializer.Serialize(incomingData);
      var body = Encoding.UTF8.GetBytes(serializedData);


      // Act
      await service.IntakeData(incomingData);

      // Assert - Verify data was inserted into MongoDB
      _mockCollection.Verify(
        c => c.InsertOneAsync(It.Is<IncomingData>(d => d.Equals(incomingData)),
          It.IsAny<InsertOneOptions>(),
          It.IsAny<System.Threading.CancellationToken>()),
        Times.Once);

      // Assert - Verify message was published to RabbitMQ
      Assert.IsTrue(_isPublished);
    }
  }
}