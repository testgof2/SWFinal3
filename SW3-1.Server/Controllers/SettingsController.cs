using Microsoft.AspNetCore.Mvc;
using SW3_1.Server.Models;
using SW3_1.Server.Services;

namespace SW3_1.Server.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class SettingsController : ControllerBase
  {
    private readonly ILogger<SettingsController> _logger;
    private readonly IDeviceAndSettingsService _deviceAndSettingsService;
    private readonly IAnalyzedDataService _analyzedDataService;

    public SettingsController(ILogger<SettingsController> logger,
      IDeviceAndSettingsService deviceAndSettingsService,
      IAnalyzedDataService analyzedDataService)
    {
      _logger = logger;
      _deviceAndSettingsService = deviceAndSettingsService;
      _analyzedDataService = analyzedDataService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceAndSettingsWithAnalyzedData>>> Get()
    {
      var res = (await _deviceAndSettingsService.GetDeviceInfoAsync())
        .Select(x => new DeviceAndSettingsWithAnalyzedData(x)).ToList();
      for (int i = 0; i < res.Count; i++)
      {
        if (res[i].LatestData.Any())
        {
          res[i].AnalyzedData = await _analyzedDataService.AnalyzeData(res[i]);
        }
      }

      return Ok(res);
    }

    [HttpPut]
    public async Task<ActionResult> Edit(DeviceAndSettings dto)
    {
      await _deviceAndSettingsService.EditDevice(dto);
      return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> Create(DeviceAndSettings dto)
    {
      await _deviceAndSettingsService.CreateNewDevice(dto);
      return Ok();
    }

    [HttpDelete]
    [Route("{deviceId}")]
    public async Task<ActionResult> Delete(Guid deviceId)
    {
      await _deviceAndSettingsService.DeleteDevice(deviceId);
      return Ok();
    }
  }
}