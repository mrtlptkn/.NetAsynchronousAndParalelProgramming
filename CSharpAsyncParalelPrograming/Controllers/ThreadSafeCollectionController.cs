using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CSharpAsyncParalelPrograming.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class ThreadSafeCollectionController : ControllerBase
  {

    [HttpPost("ConcurrentDictionarySample")]
    public async Task<IActionResult> ConcurrentDictionarySample()
    {
      LearningLabs.ThreadSafeCollectionSample.ConcurrentDictionarySample();

      return Ok();
    }

    [HttpPost("ConcurentBag")]
    public async Task<IActionResult> ConcurentBag()
    {
      LearningLabs.ThreadSafeCollectionSample.ConcurrentBagSample();

      return Ok();
    }

    [HttpPost("NormalDictionarySample")]
    public async Task<IActionResult> NormalDictionarySample()
    {
      LearningLabs.ThreadSafeCollectionSample.NormalDictionarySample();

      return Ok();
    }

  }
}
