using System.Diagnostics;

namespace CSharpAsyncParalelPrograming.LearningLabs
{

  public static class TaskSample
  {


    // YANLIŞ KULLANIM örneği: Fire-and-forget + await edilmeyen Task.Delay.
    // Sonuç: metot 2 sn BEKLEMEZ, hemen döner; Task içindeki bir exception da kaybolur (kimse gözlemlemez).
    public static async Task FireAndForgetTask()
    {

      Console.WriteLine($"[Main Thread] {Thread.CurrentThread.ManagedThreadId}");

      // 1) Task.Run await edilmediği için fire-and-forget: çağıran beklemeden ilerler.
      _ = Task.Run(async () =>
      {
        Console.WriteLine($"[Task] Başladı ID: {Thread.CurrentThread.ManagedThreadId}");
      });

      await Task.CompletedTask;
    }


    public static async Task DelayedTask()
    {

      Console.WriteLine($"[Main Thread] {Thread.CurrentThread.ManagedThreadId}");

      // await, thread'i bloklamaz: Task tamamlanana kadar metot askıya alınır, thread başka işlere (thread pool'a) döner.
      // Task tamamlanınca metot kaldığı yerden devam eder.
      await Task.Run(async () =>
      {
        Console.WriteLine($"[Task] Başladı ID: {Thread.CurrentThread.ManagedThreadId}");
        await Task.Delay(2000);
      });

    }


    // Task iptal işlemleri ve Hata Yönetimi
    // 1. yöntem olsun
    public static async Task TaskCancelation()
    {
      using CancellationTokenSource cts = new CancellationTokenSource();
      cts.CancelAfter(30); // 3 saniye sonra iptal et

      // CancellationTokenSource iptal işlemlerini yönetir; CancellationToken ise görevlere verilir ve iptal sinyalini taşır.
      // İptalin Task'e "Canceled" olarak yansıması için görev ThrowIfCancellationRequested ile OperationCanceledException fırlatmalıdır.
      // Sadece "return" ederse Task RanToCompletion olur ve çağıran iptali fark etmez.

      Task longRunningTask = Task.Run(async () =>
      {
        for (int i = 0; i < 10; i++)
        {
          cts.Token.ThrowIfCancellationRequested();
          Console.WriteLine($"Çalışıyor... {i}");
          await Task.Delay(1000, cts.Token); // token verildiği için bekleme anında kesilir
        }
      }, cts.Token);

      try
      {
        await longRunningTask; // longRunningTask tamamlanana kadar bekleriz. Eğer iptal edilmişse, OperationCanceledException fırlatılır.
      }
      catch (OperationCanceledException) // TaskCanceledException da bunun alt sınıfıdır
      {
        Console.WriteLine($"Task iptal edildiği için yakalandı. Durum: {longRunningTask.Status}"); // Canceled
      }
    }


    public static async Task TaskException()
    {
      // 1) Tek task: await, exception'ı orijinal tipiyle fırlatır.
      try
      {
        await Task.Run(() =>
        {
          throw new InvalidOperationException("Task içinde bir istisnai durum meydana geldi");
        });
      }
      catch (InvalidOperationException ex)
      {
        Console.WriteLine($"[Tek Task] Yakalandı: {ex.Message}");
      }

      // 2) WhenAll: birden fazla task hata verirse await SADECE İLK exception'ı fırlatır.
      //    Hepsine task.Exception.InnerExceptions üzerinden ulaşılır.
      Task t1 = Task.CompletedTask; //Task.FromException(new InvalidOperationException("Hata 1"));
      Task t2 = Task.FromException(new ArgumentException("Hata 2"));
      
      try
      {
        await Task.WhenAll(t1, t2);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[WhenAll] await'in fırlattığı: {ex.Message}");
      }
    }


    // Task Methodları (WhenAny,WhenAll,ContinueWith), ValueTask, Methodu Yapay zekaya yorumlatma işlemi 
    public static async Task TaskWhenAll()
    {
      // eğer birden fazla taskı aynı anda başlatmak istiyorsak ve hepsinin sonucunu beklemek istiyorsak Task.WhenAll methodunu kullanabiliriz. Bu method, verilen taskların tümü tamamlandığında tamamlanır ve sonuçlarını bir dizi olarak döndürür.

      Stopwatch sp = Stopwatch.StartNew();


      Task<int> task1 = Task.Run(async () =>
      {
        await Task.Delay(2000);
        Console.WriteLine("[Task1]" + Thread.CurrentThread.ManagedThreadId);
        return 1;

      });

      Task<int> task2 = Task.Run(async () =>
      {
        await Task.Delay(5000);
        Console.WriteLine("[Task2]" + Thread.CurrentThread.ManagedThreadId);
        return 2;

      });


      Task<string> task3 = Task.Run(async () =>
      {
        await Task.Delay(3000);
        Console.WriteLine("[Task3]" + Thread.CurrentThread.ManagedThreadId);
        return "Task 3 tamamlandı";

      });

      // Sıralı (senkron) çalışsaydı toplam 2 + 5 + 3 = 10 sn sürerdi.
      // Paralel çalıştığı için toplam ≈ 5 sn olur (en uzun task: task2).
      // Task.WhenAll tüm taskların tamamlanmasını bekler. Aynı tipli Task<T>'ler için sonuçları dizi olarak döndürür (var r = await Task.WhenAll(...)).
      // Hata durumunda await ilk exception'ı fırlatır; hepsi için task.Exception.InnerExceptions'a bakılır.

      // JS deki Promise.all benzer.
      // bütün işlemlerin aynı anda çözümlenip, response kullanılması gereken bir durum varsa mantıklı
      await Task.WhenAll(task1, task2, task3);
      Console.WriteLine($"Tüm tasklar tamamlandı. Toplam süre: {sp.ElapsedMilliseconds} ms");

    }

    public static async Task TaskWhenAny()
    {
      // Task.WhenAny, verilen taskların İLK biten hâliyle tamamlanır ve bitmiş task'ı döndürür.
      // Diğer tasklar iptal edilmez, arka planda çalışmaya devam eder.

      Stopwatch sp = Stopwatch.StartNew();


      Task<int> task1 = Task.Run(async () =>
      {
        await Task.Delay(2000);
        Console.WriteLine("[Task1]" + Thread.CurrentThread.ManagedThreadId);
        return 1;

      });

      Task<int> task2 = Task.Run(async () =>
      {
        await Task.Delay(5000);
        Console.WriteLine("[Task2]" + Thread.CurrentThread.ManagedThreadId);
        return 2;

      });


      Task<string> task3 = Task.Run(async () =>
      {
        await Task.Delay(3000);
        Console.WriteLine("[Task3]" + Thread.CurrentThread.ManagedThreadId);
        return "Task 3 tamamlandı";

      });

      // JS'deki Promise.race benzer. En hızlı kaynağı seçmek veya timeout uygulamak için kullanılır.
      Task ilkBiten = await Task.WhenAny(task1, task2, task3);
      Console.WriteLine($"İlk biten task: {(ilkBiten == task1 ? "task1" : ilkBiten == task2 ? "task2" : "task3")}. Süre: {sp.ElapsedMilliseconds} ms"); // ≈ 2000 ms


    }

    /// <summary>
    /// ValueTask<T> bir metodun hem senkron hem asenkron dönebilmesini sağlar. Ama asıl amacı senkron yolu hızlandırmaktır, yani sonuç çoğu zaman hemen hazırsa gereksiz bellek ayırmayı önlemek.
    ///  Birden fazla kez kullanılacak veya Task.WhenAll ile birleştirilecekse .AsTask() çağırın.
    /// </remarks>
    public static async ValueTask<int> ValueTaskExample(bool cached)
    {

      if (cached)
        return 100; // Senkron yol: Task nesnesi oluşmaz

      await Task.Delay(2000); // Asenkron yol: gerçek bir bekleme

      return 100;

    }


  }
}
