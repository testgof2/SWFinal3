using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SW3_DataCollector.Services;
using SW3_shared.Models;

namespace SW3_DataCollector.Controllers
{
  [ApiController]
  [Route("[controller]")]
  [AllowAnonymous]
  public class DataIntakeController : ControllerBase
  {

    private readonly ILogger<DataIntakeController> _logger;
    private readonly IDataIntakeService _dataIntakeService;

    public DataIntakeController(ILogger<DataIntakeController> logger, IDataIntakeService dataIntakeService)
    {
      _logger = logger;
      _dataIntakeService = dataIntakeService;
    }

    [HttpPost]
    public async Task<IActionResult> Post(IncomingData dto)
    {
      await _dataIntakeService.IntakeData(dto);
      return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
   
      return Ok();
    }
  }
}
