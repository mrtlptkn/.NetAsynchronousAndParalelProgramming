using CSharpAsyncParalelPrograming.LearningLabs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CSharpAsyncParalelPrograming.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AsyncLabsController : ControllerBase
  {

    private readonly TaskService _taskService;

    public AsyncLabsController(TaskService taskService)
    {
      this._taskService = taskService;
    }


    [HttpPost("FireAndForgetTask")]
    public async Task<IActionResult> FireAndForgetTask()
    {
      await LearningLabs.TaskSample.FireAndForgetTask();
      return Ok("FireAndForgetTask");

    }

    [HttpPost("DelayedTask")]

    public async Task<IActionResult> DelayedTask()
    {
      await LearningLabs.TaskSample.DelayedTask();
      return Ok("DelayedTask");
    }



    // Uyarı: Servis 2 snlik bir bekleme süresi sonucunda ilgili requesti handle edebiliyor. Aşağıdaki kod hiç bir şekilde resultı beklemediği için response.IsCompleted state geçemez bu durumda istek cevap olarak yanlış sonuç döndürür. 

    [HttpPost("CustomTaskService")]
    public async Task<IActionResult> CustomTaskService(CancellationToken cancellationToken)
    {
      // response bu aşamada çözümlenmiş olmuyor. asenkron state de aslında dönüş oluyor
      
      var response =  await _taskService.GetUsersAsync(cancellationToken);
      return Ok(response);
    }


    // Client uzun süren isteği iptal edersek
    // Web uygulamalarında iptal sinyali CancellationToken  cancellationToken
    [HttpGet("ClientCancelRequest")]
    public async Task<IActionResult> ClientCancelRequest(CancellationToken cancellationToken)
    {

      await _taskService.GetUsersAsync(cancellationToken);

      return Ok("TaskCancelation");

    }

    [HttpPost("TaskCancelation")]
    public async Task<IActionResult> TaskCancelation()
    {
      await LearningLabs.TaskSample.TaskCancelation();

      return Ok("TaskCancelation");

    }

    [HttpPost("TaskException")]
    public async Task<IActionResult> TaskException()
    {
      try
      {
        await LearningLabs.TaskSample.TaskException();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      return Ok();
    }


    [HttpPost("TaskContinueWith")]
    public async Task<IActionResult> TaskContinueWith()
    {

      // ContinueWith aynı Task'a birden fazla bağımsız devam eklemeniz gerektiğinde ya da koşullu bir akış kurmanız gerektiğinde.
      var taskState = LearningLabs.TaskSample.TaskException().ContinueWith(async (task) =>
      {

        // Exception varsa hatayı yakalarız.
        if (task.IsFaulted)
        {
          Console.WriteLine($"[Error3]  ${task.Exception.Message};");
        }

      });

      // return ok beklemez sonuç hızlı döner ama background iş yapılır. 
      return Ok();

    }


    [HttpPost("TaskWhenAll")]
    public async Task<IActionResult> TaskWhenAll()
    {

      await LearningLabs.TaskSample.TaskWhenAll();

      return Ok();
    }

    [HttpPost("TaskWhenAny")]
    public async Task<IActionResult> TaskWhenAny()
    {

      await LearningLabs.TaskSample.TaskWhenAny();

      return Ok();
    }


  }
}
