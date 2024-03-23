using AutoFixture;
using MongoDB.Driver;
using Moq;
using SW3_1.Server.Models;
using SW3_1.Server.Services;
using SW3_shared;

namespace SettingsServerTest.UnitTests
{
  [TestClass]
  public class DeviceAndSettingsServiceTests
  {
    private Mock<IMongoDatabase> _mockMongoDb;
    private Mock<IMongoCollection<DeviceAndSettings>> _mockCollection;
    private IFixture _fixture;
    private DeviceAndSettingsService _service;
    private Mock<IRepository> _repository;

    [TestInitialize]
    [TestCategory("Unit")]
    public void Initialize()
    {
      _mockMongoDb = new Mock<IMongoDatabase>();
      _mockCollection = new Mock<IMongoCollection<DeviceAndSettings>>();
      _repository = new Mock<IRepository>();


      _fixture = new Fixture();

      _mockMongoDb.Setup(db => db.GetCollection<DeviceAndSettings>(It.IsAny<string>(), null))
        .Returns(_mockCollection.Object);


      _repository.Setup(x => x.ExposeOriginalClient()).Returns(_mockMongoDb.Object);

      _service = new DeviceAndSettingsService(_repository.Object);
    }

    [TestMethod]
    public async Task CreateNewDevice_InsertsDevice()
    {
      // Arrange
      var device = _fixture.Create<DeviceAndSettings>();

      // Act
      await _service.CreateNewDevice(device);

      // Assert
      _mockCollection.Verify(col => col.InsertOneAsync(device, null, default), Times.Once);
    }

    [TestMethod]
    public async Task EditDevice_ReplacesDevice()
    {
      // Arrange
      var device = _fixture.Create<DeviceAndSettings>();

      // Act
      await _service.EditDevice(device);

      // Assert
      _mockCollection.Verify(col => col.ReplaceOneAsync(
          It.IsAny<FilterDefinition<DeviceAndSettings>>(),
          device,
          It.IsAny<ReplaceOptions>(),
          default),
        Times.Once);
    }


    [TestMethod]
    public async Task GetDeviceInfoAsync_ReturnsDevices()
    {
      var devices = _fixture.CreateMany<DeviceAndSettings>();
      _repository.Setup(c => c.FindAll<DeviceAndSettings>()).ReturnsAsync(devices);

      // Act
      var result = await _service.GetDeviceInfoAsync();

      // Assert
      Assert.AreEqual(devices.Count(), result.Count());
    }
  }
}