using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAsyncParalelPrograming.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class RaceConditionsController : ControllerBase
  {

    // Race Condition
    [HttpPost("RaceConditionDemo")]
    public IActionResult RaceConditionDemo()
    {
      LearningLabs.RaceConditionSample.RaceConditionDemo();

      return Ok();
    }


    [HttpPost("InterlockDemo")]
    public IActionResult InterlockDemo()
    {
      LearningLabs.RaceConditionSample.InterlockDemo();

      return Ok();
    }


    [HttpPost("MonitorDemo")]
    public IActionResult MonitorDemo()
    {
      LearningLabs.RaceConditionSample.MonitorDemo();

      return Ok();
    }

    [HttpPost("MonitorDemo2")]
    public IActionResult MonitorDemo2()
    {
      LearningLabs.RaceConditionSample.MonitorDemo2();

      return Ok();
    }

    [HttpPost("MutexDemo")]
    public IActionResult MutexDemo()
    {
      LearningLabs.RaceConditionSample.MutexDemo();

      return Ok();
    }


    [HttpPost("SemaphoreSlimSample")]
    public async Task<IActionResult> SemaphoreSlimSample()
    {
      await LearningLabs.RaceConditionSample.SemaphoreSlimSample();

      return Ok();
    }


    [HttpPost("ReaderWriterLockSlimSample")]
    public async Task<IActionResult> ReaderWriterLockSlimSample()
    {
      LearningLabs.RaceConditionSample.ReaderWriterLockSlimSample();

      return Ok();
    }


  }
}
