using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAsyncParalelPrograming.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ParallelsController : ControllerBase
  {


    [HttpPost("ParalelForVsNormalFor")]
    public IActionResult ParalelForVsNormalFor()
    {

      LearningLabs.ParallelProgrammingSample.ParallelForVsNormalFor();

      return Ok();
    }

    [HttpPost("ParallelForeachCancelationTokenAndParalelOptions")]
    public IActionResult ParallelForeachCancelationTokenAndParalelOptions()
    {

      LearningLabs.ParallelProgrammingSample.ParallelForeachCancelationTokenAndParalelOptions();

      return Ok();
    }


    [HttpPost("ParallelForWithThreadLocalState")]
    public IActionResult ParallelForWithThreadLocalState()
    {

      LearningLabs.ParallelProgrammingSample.ParallelForWithThreadLocalState();

      return Ok();
    }


    [HttpPost("ParallelInvokeSample")]
    public IActionResult ParallelInvokeSample()
    {

      LearningLabs.ParallelProgrammingSample.ParallelInvokeSample();

      return Ok();
    }

    [HttpPost("ParallelAsyncExceptionHandling")]
    public async Task<IActionResult> ParallelAsyncExceptionHandling()
    {
      await LearningLabs.ParallelProgrammingSample.ParallelAsyncExceptionHandling();

      return Ok();
    }



    [HttpPost("PLINQSample")]
    public IActionResult PLINQSample()
    {
      LearningLabs.ParallelProgrammingSample.PLINQSample();

      return Ok();
    }
  }
}
