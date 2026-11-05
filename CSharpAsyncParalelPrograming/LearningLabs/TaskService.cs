namespace CSharpAsyncParalelPrograming.LearningLabs
{

  public class TaskService
  {
   

    // Burada Client isteği iptal ettğini var sayıyoruz.
    // Arka planda uzun çalışan iş isteklerini iptal edilebilir hale getirmek.

    public async Task<List<UserResponse>> GetUsersAsync(CancellationToken cancellationToken)
    {
      try
      {

        // Api Call işlemi var.
        // Tam bu bekleme aşamasında Client isteği iptal ederse ne olur ?
        // Token Task.Delay'e verildiği için iptal anında beklemeyi keser ve TaskCanceledException fırlatır.
        // (Token verilmezse 5 sn dolana kadar iptal fark edilmez.)
        await Task.Delay(5000, cancellationToken);

        // Bu noktaya gelindiyse hâlâ iptal edilmemiştir. Db işlemine başlamadan önce bir kez daha kontrol edebiliriz.
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine($"[TaskService] {Thread.CurrentThread.ManagedThreadId}");

        // Db Save Changes (gerçekte EF Core: await db.SaveChangesAsync(cancellationToken)).
        // I/O işlemi için Task.Run kullanmıyoruz; zaten async olan işi thread pool'a taşımak gereksizdir.
        var response = new List<UserResponse> { new UserResponse { Name = "Ali" }, new UserResponse { Name = "Cenk" } };
        Console.WriteLine($"[Operation is successfull] {response.Count} kullanıcı");

        return response;

      }
      catch (OperationCanceledException ex)
      {
        Console.WriteLine($"[Operation is canceled] {ex.Message}");

        // Loglayıp yeniden fırlatıyoruz: null dönmek çağıranın iptali fark etmesini engeller.
        // ASP.NET Core, iptal edilen istekte bu exception'ı kendisi yönetir.
        throw;
      }


    }



    // Sync-over-async: async bir metodu senkron bir kod bloğundan çağırmak zorunda kaldığımızda (örn. Main'in async olamadığı eski kod) kullanılır.
    // Tüm bu yöntemler (GetAwaiter().GetResult(), .Result, .Wait(), Task.WaitAll) çağıran thread'i BLOKE eder; mümkünse kaçının.
    //  - GetAwaiter().GetResult(): hatayı AggregateException'a sarmadan orijinal exception olarak fırlatır (3 yöntem içinde en iyisi).
    //  - .Result / .Wait(): hatayı AggregateException içinde fırlatır.
    //  - Task.WaitAsync(...) bloke etmez; bir Task döner (timeout/iptal eklemek için).
    // Deadlock: SynchronizationContext olan ortamlarda (WinForms/WPF/klasik ASP.NET) await devamı UI/request thread'ine dönmek ister,
    // o thread ise .Result ile bekliyordur -> kilitlenme. ASP.NET Core'da SynchronizationContext yoktur; deadlock yerine
    // thread pool starvation (thread'ler bloklanıp havuz tükenir) yaşanır.
    public List<UserResponse> GetUsers()
    {
      // Gerçek bir async iş: await ile devam eden bir metot. Bloke ederek sonucunu alıyoruz.
      return GetUsersCoreAsync().GetAwaiter().GetResult();

      // Aynı iş diğer yöntemlerle:
      // return GetUsersCoreAsync().Result;          // AggregateException sarmalı
      // GetUsersCoreAsync().Wait();                  // sonuç Task.Result ile alınır
    }

    private static async Task<List<UserResponse>> GetUsersCoreAsync()
    {
      await Task.Delay(500);
      Console.WriteLine($"[Blocked TaskService] {Thread.CurrentThread.ManagedThreadId}");
      return new List<UserResponse> { new UserResponse { Name = "Ali" }, new UserResponse { Name = "Cenk" } };
    }

  }
}
