using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture.AutoMoq;
using AutoFixture;
using MongoDB.Driver;
using Moq;
using SW3_1.Server.Models;
using SW3_1.Server.Services;
using SW3_shared;
using SW3_shared.Models;

namespace SettingsServerTest.UnitTests
{
  [TestClass]
  public class DeviceAndSettingsTests
  {
    [TestMethod]
    [TestCategory("Unit")]
    public async Task HandleIncomingData_UpdatesLatestData_KeepsOnlyTenEntries()
    {
      var fixture = new Fixture().Customize(new AutoMoqCustomization());
      // Arrange
      var repositoryMock = new Mock<IRepository>(); // Adjust IYourRepositoryType to your repository interface
      var mongoDb = new Mock<IMongoDatabase>();

      var mockCollection = new Mock<IMongoCollection<DeviceAndSettings>>();

      mongoDb.Setup(x => x.GetCollection<DeviceAndSettings>(nameof(DeviceAndSettings), null))
        .Returns(mockCollection.Object);

      repositoryMock.Setup(x => x.ExposeOriginalClient()).Returns(mongoDb.Object);

      var deviceId = Guid.NewGuid();
      var initialData = Enumerable.Range(1, 10).Select(x =>
        new DataWithTime
          { CreatedAt = DateTime.UtcNow, Values = new List<float>(x) }).ToList(); // Assuming 9 entries initially
      var newData = new IncomingData { DeviceId = deviceId, Data = new List<float> { 11f } }; // New data to be added


      var deviceAndSettings = fixture.Build<DeviceAndSettings>().With(x => x.LatestData, initialData)
        .With(x => x.Id, deviceId).Create();


      repositoryMock.Setup(r => r.FindAll<DeviceAndSettings>())
        .ReturnsAsync(new List<DeviceAndSettings> { deviceAndSettings });

      var service =
        new AnalyzedDataService(repositoryMock.Object); // Replace YourServiceClass with the actual service class name

      // Act
      await service.HandleIncomingData(newData);

      // Assert
      Assert.AreEqual(10, deviceAndSettings.LatestData.Count()); // Check that LatestData now has exactly 10 entries

      Assert.IsFalse(deviceAndSettings.LatestData.SelectMany(x => x.Values)
        .Contains(1f)); // The oldest data (1f) should be removed
      Assert.IsTrue(deviceAndSettings.LatestData.SelectMany(x => x.Values).ToList()
        .Contains(11f)); // Ensure the new data is added
    }


    [TestMethod]
    public async Task AnalyzeData_ReturnsCorrectAnalyzedData()
    {
      var fixture = new Fixture().Customize(new AutoMoqCustomization());
      // Arrange
      var repositoryMock = new Mock<IRepository>(); // Adjust IYourRepositoryType to your repository interface
      var mongoDb = new Mock<IMongoDatabase>();
      var mockCollection = new Mock<IMongoCollection<DeviceAndSettings>>();

      mongoDb.Setup(x => x.GetCollection<DeviceAndSettings>(nameof(DeviceAndSettings), null))
        .Returns(mockCollection.Object);

      var service =
        new AnalyzedDataService(repositoryMock.Object);


      var annotations = new List<DataAnnotations>
      {
        new() { Name = "Temperature", Unit = "Celsius" },
        // Add more data annotations as needed
      };
      var latestData = new List<DataWithTime>
      {
        new() { Values = new List<float> { 1, 2 } },
        new() { Values = new List<float> { 3, 4 } },
        // Add more data points as needed
      };

      // Arrange
      var deviceAndSettings = fixture.Build<DeviceAndSettings>()
        .With(x => x.LatestData, latestData).With(x => x.DataAnnotations, annotations).Create();


      var expectedAnalyzedData = new List<AnalyzedData>
      {
        new() { Average = 2, Median = 2 },
      };


      var result = await service.AnalyzeData(deviceAndSettings);

      // Assert
      Assert.IsNotNull(result);
      var resultList = result.ToList();
      Assert.AreEqual(expectedAnalyzedData.Count, resultList.Count);
      for (int i = 0; i < expectedAnalyzedData.Count; i++)
      {
        Assert.AreEqual(expectedAnalyzedData[i].Average, resultList[i].Average);
        Assert.AreEqual(expectedAnalyzedData[i].Median, resultList[i].Median);
      }
    }
  }
}