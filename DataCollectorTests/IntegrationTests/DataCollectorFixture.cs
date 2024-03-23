
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SW3_DataCollector;

namespace DataCollectorTests.IntegrationTests
{
  public class DataCollectorFixture : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
  
      var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

      builder
        // This configuration is used during the creation of the application
        // (e.g. BEFORE WebApplication.CreateBuilder(args) is called in Program.cs).
        .UseConfiguration(configuration)
        .ConfigureAppConfiguration(configurationBuilder =>
        {
          // This overrides configuration settings that were added as part 
          // of building the Host (e.g. calling WebApplication.CreateBuilder(args)).
          configurationBuilder.AddJsonFile("appsettings.json");
        });
    }
  }

}
