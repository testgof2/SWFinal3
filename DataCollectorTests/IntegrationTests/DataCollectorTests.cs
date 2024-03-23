using System.Net.Http.Json;
using System.Text.Json;
using AutoFixture.AutoMoq;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using SW3_DataCollector;
using SW3_shared.Models;
using Xunit;

namespace DataCollectorTests.IntegrationTests
{
  [TestClass]
  [TestCategory("Integration")]
  public class DataCollectorTests : IClassFixture<WebApplicationFactory<Program>>
  {
    private readonly DataCollectorFixture _factory;

    public DataCollectorTests()
    {
      _factory = new DataCollectorFixture();
    }


    [TestMethod]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType()
    {

      var client = _factory.CreateClient();

      var fixture = new Fixture().Customize(new AutoMoqCustomization());
      var data = fixture.Build<IncomingData>().Create();


      var response = await client.PostAsJsonAsync(new Uri("http://localhost/DataIntake"), data);
      response.EnsureSuccessStatusCode(); 
    }
  }
}