using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSharpAsyncParalelPrograming.LearningLabs
{
  public static class ParallelProgrammingSample
  {

    // Parallel CPU tüketiminin yoğun olduğu durumlarda daha performanslıdır
    public static void ParallelForVsNormalFor()
    {

      Stopwatch paralelSp = Stopwatch.StartNew();

      Parallel.For(0, 10000, (i) =>
      {
        Thread.SpinWait(1000); // CPU bounded iş simülasyonu
      });

      paralelSp.Stop();
      Console.WriteLine($"paralelFor Time {paralelSp.ElapsedMilliseconds} ms");


      Stopwatch forSp = Stopwatch.StartNew();

      for (int i = 0; i < 10000; i++)
      {
        Thread.SpinWait(1000); // CPU bounded iş simülasyonu
      }

      forSp.Stop();
      Console.WriteLine($"For Sp Time {forSp.ElapsedMilliseconds} ms");

    }

    // Ramdeki milyonlarca veri üzerinde CPU-bound işlemi farklı threadler ile paralel de uygulamak ve kodun hesaplama hızını ciddi oranda artırmak için kullanılabilir. 
    // Not: MaxDegreeOfParallelism eğer emin değilseniz bunun dinamik olarak ThreadPool üzerinden .NET tarafından karar verilmesine izin verin. Cancelation Token önemli uzun süre long running process bu sebeple iptal durumu olmaz ise, threadlerin hepsinin işi bitirmesini bekletir. 

    public static void ParallelForeachCancelationTokenAndParalelOptions()
    {
      List<string> sehirler = ["İstanbul", "İzmir", "Ankara", "Manisa", "Bursa", "Konya", "Antalya"];

      // unmanagement bir resource using kullanalım
      using CancellationTokenSource cts = new CancellationTokenSource();

      ParallelOptions parallelOptions = new ParallelOptions
      {
        CancellationToken = cts.Token,
        MaxDegreeOfParallelism = 3 // bu iş için maksimum 3 thread kullan

      };

      // İlk döngü iptalden önce çalışır (iptal henüz istenmedi).
      Parallel.ForEach(sehirler, parallelOptions, sehir =>
      {
        Console.WriteLine($"[Paralel Foreach] {sehir} tamamlandı, Thread: {Thread.CurrentThread.ManagedThreadId}");
      });


      // Paralel işlemi iptal etmek için ParallelOptions içinde CancellationToken ayarlanır.
      // İptali deterministik göstermek için i == 50 olduğunda cts.Cancel() çağırıyoruz (zamanlayıcıya bağımlı değil).
      try
      {
        Parallel.ForEach(Enumerable.Range(0, 100000), parallelOptions, i =>
        {
          if (i == 50)
            cts.Cancel();
        });

      }
      catch (OperationCanceledException ex)
      {
        Console.WriteLine($"İptal edildi: {ex.Message}");
      }

    }



    // amaç: shared bir data, ortak kullanılan bir değişkenin, farklı threadlerin sonucu bozmadan (race condition) olmadan çözmek.
    public static void ParallelForWithThreadLocalState()
    {

      long toplamSonuc = 0;

      // Her thread kendi localSum değişkeninde toplar (body içinde paylaşılan veriye dokunulmaz, kilit gerekmez).
      // localFinally ise her worker için bitişte ÇAĞRILIR ve worker'lar eşzamanlı çalışabilir.
      // Bu yüzden paylaşılan toplamSonuc'a birleştirme thread-safe olmalıdır: Interlocked.Add (veya lock).

      Parallel.For<long>(fromInclusive: 0, toExclusive: 100, 
        localInit: () => 0L, 
        body: (i, state, localSum) => localSum + i, 
        localFinally: localSum =>
      {
    
        Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}, [Thread] Local Toplam: {localSum}");
        Interlocked.Add(ref toplamSonuc, localSum);
      });


      Console.WriteLine("[Thread] Local Toplam: " + toplamSonuc);

    }

    // Birbirinden bağımsız bir şekilde olan işlemleri ise Paralel Invoke ile yönetebiliriz. Örneğin, üç farklı işlemi aynı anda yürütmek istiyorsak, Parallel.Invoke kullanarak bu işlemleri paralel olarak çalıştırabiliriz. Bu yöntem, işlemlerin birbirinden bağımsız olduğu durumlarda performansı artırabilir.


    public static void ParallelInvokeSample()
    {

      Stopwatch sp = Stopwatch.StartNew();

      // senkron methodlar
      // methodlar senkron olmasına rağmen paralel işlemk için kullanabiliriz.
      var action1 = new Action(() =>
      {
        Console.WriteLine($"[Paralel Invoke] İşlem 1 - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(1000); // Simulate work
        Console.WriteLine($"[Paralel Invoke] İşlem 1 tamamlandı");
      });


      var action2 = new Action(() =>
      {
        Console.WriteLine($"[Paralel Invoke] İşlem 2 - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(2000); // Simulate work
        Console.WriteLine($"[Paralel Invoke] İşlem 2 tamamlandı");
      });


      var action3 = new Action(() =>
      {
        Console.WriteLine($"[Paralel Invoke] İşlem 3 - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
        Thread.Sleep(2000); // Simulate work
        Console.WriteLine($"[Paralel Invoke] İşlem 3 tamamlandı");
      });



      Parallel.Invoke(action1, action2, action3);

      sp.Stop();

      Console.WriteLine($"[Paralel Invoke] Tüm işlemler tamamlandı - Thread ID: {Thread.CurrentThread.ManagedThreadId}");
      Console.WriteLine($"[Paralel Invoke] Toplam süre: {sp.ElapsedMilliseconds} ms (≈2000, sıralı olsaydı ≈5000)");

    }


    // Paralel Exception Handling

    public static async Task ParallelAsyncExceptionHandling()
    {
      // Paralel.ForAsync, belirli bir aralıkta işlemleri paralel olarak yürütmek için kullanılır. Bu yöntem, genellikle döngülerdeki işlemleri hızlandırmak için tercih edilir. Örneğin, büyük bir dizi üzerinde işlem yaparken, her bir öğeyi paralel olarak işleyebilirsiniz. Ancak, bu yöntem asenkron işlemlerle çalışırken daha uygun olabilir.
      Stopwatch sp = Stopwatch.StartNew();

      using CancellationTokenSource cts = new CancellationTokenSource();
      //cts.CancelAfter(300); // 300ms saniye sonra iptal uygula.); alt async methodda iptal durumunuda kontrol edelim. 

      ParallelOptions parallelOptions = new ParallelOptions();
      parallelOptions.CancellationToken = cts.Token;

      try
      {
        await Parallel.ForAsync(0, 10, parallelOptions, async (i, cancellationToken) =>
        {
          // Cpu-bound işlemler
          double result = Math.Sqrt(i) * Math.PI * Math.Pow(2, i + 1);
          // Ağ gecikmesi simülasyonu

          await SampleTaskAsync(result, cancellationToken); // asenkron kod bloğunda hata oluşmalı.
          // await ile asenkron bir iş yapmamızıa da imkan sağlıyor.
        });
      }
      catch (OperationCanceledException ex)
      {
        Console.WriteLine("İptal Edildi: " + ex.Message);
      }
      catch (Exception ex)
      {
        // Diğer tüm exception'ları yakala
        Console.WriteLine("Beklenmeyen Hata: " + ex.Message);
      }

    }

    public async static Task SampleTaskAsync(double result, CancellationToken cancellationToken = default)
    {
      int gecikme = Random.Shared.Next(10, 100); // 10 ile 100 ms arasında rastgele gecikme
      await Task.Delay(gecikme, cancellationToken); // Simulate asynchronous work
      Console.WriteLine($"Result {result} - Thread ID: {Thread.CurrentThread.ManagedThreadId}");

      // Eğer Task içerisinde bir exception durumu meydana gelirse.
      if (gecikme > 20)
      {
        throw new InvalidOperationException("Task hata oluşturdu");
      }
    }



    // PLINQ -> LINQ sorgularını paralel olarak yürütmek için kullanılan bir özelliktir. PLINQ, büyük veri setleri üzerinde sorguları hızlandırmak için kullanılabilir. Örneğin, bir dizi üzerinde filtreleme, sıralama veya gruplama işlemlerini paralel olarak gerçekleştirebilirsiniz. PLINQ, sorguların bağımsız olduğu durumlarda performansı artırabilir, ancak dikkatli yönetilmezse yarış koşulları ve diğer sorunlara yol açabilir. Bu nedenle, PLINQ kullanırken senkronizasyon ve hata yönetimi konularına dikkat etmek önemlidir.


    // Biz veritabanında paralel sorgu çalıştırmıyoruz, biz program tarafında veritabanında çekilen milyonlarca veriyi, program tarafında parlale olarak işlemeye çalışıyoruz. 
    // PLINQ kullanınca, veri işlem işlemleri performans amaçlı, varsayılan olarak sırasız çalışır. asOrdered zorlarsak performanstan ödün veriririz. 

    public static void PLINQSample()
    {
      using CancellationTokenSource cts = new CancellationTokenSource();
      //cts.CancelAfter(300);


      // Not: Paralel lambda içinden paylaşılan bir List<T>'e yazmak concurrency problemi çıkarır. PLINQ'da sonuçları
      // yan etkiyle toplamak yerine Select/ToList/ToArray ile sorgunun sonucu olarak alın (veya ConcurrentBag kullanın).
      ConcurrentBag<Employee> employees = new();

      Parallel.For(0, 100, (i) =>
      {
        employees.Add(new Employee
        {
          Id = i,
          Name = $"Employee {i}",
          Salary = Random.Shared.Next(30000, 500000)
        });
      });

      try
      {
        // PLINQ kullanarak yüksek maaşlı çalışanları filtreleyelim.
        // AsParallel() sorgu birden fazla thread tarafında işlenecek
        // Not: Küçük veri kümlerinde Default ile paralel uygun değilse tek bir thread üzerinde de işi yürütebilir. ParallelExecutionMode.ForceParallelism seçerek paralel olamaya zorlamış oluyoruz. 
        // Veriyi dönüştürmek için Select ile projeksiyon yapıyoruz (orijinal nesneleri değiştirmiyoruz, yan etki yok).
        var highSalaryEmployees = employees.AsParallel().WithDegreeOfParallelism(3).WithExecutionMode(ParallelExecutionMode.ForceParallelism).WithCancellation(cts.Token)
                                           .Where(e => e.Salary > 80000)
                                           .Select(e => new { e.Id, Name = e.Name.ToUpper(), e.Salary });

        // Sorgu burada (ForAll/ToList/ToArray gibi bir terminal operatörde) çalışır.
        // Not: ForAll sıralamayı korumaz, AsOrdered() ile birlikte anlamsızdır. Sıra gerekiyorsa AsOrdered().ToList() kullanın.
        // Not: Bu kadar hafif bir filtre için PLINQ, sıralı LINQ'dan yavaş olabilir; ölçmeden varsaymayın.
        int highSalaryEmployeeCount = 0;
        highSalaryEmployees.ForAll(e =>
        {
          // Sorgu sırasında (Where/Select/ForAll içinde) fırlayan exception'lar AggregateException olarak toplanır.
          if (e.Salary > 200000)
          {
            Console.WriteLine($"[Çalışan] {e.Name},[Thread] {Thread.CurrentThread.ManagedThreadId}");
            Interlocked.Increment(ref highSalaryEmployeeCount);
          }

        });

        Console.WriteLine($"Yüksek Maaşlı çalışan sayısı: {highSalaryEmployeeCount}");
      }
      catch (OperationCanceledException ex)
      {
        Console.WriteLine(ex.Message);
      }
      catch (AggregateException ae)
      {
        Console.WriteLine("Exception Count: " + ae.InnerExceptions.Count);
      }

    }


    public class Employee
    {
      public int Id { get; set; }
      public string Name { get; set; } = string.Empty;
      public decimal Salary { get; set; }
    }



    // Farklı Threadler ile çalışırken Normal Enumarable, List gibi listelerin içerisine eleman ekleme çıkarma durumları da ciddi sorunlara neden olabilir bu durumda Concurent Collectionslardan yararlanmalıyız. Bunuda anlatalım.


  }

}
