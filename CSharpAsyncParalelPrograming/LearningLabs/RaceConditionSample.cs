using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace CSharpAsyncParalelPrograming.LearningLabs
{
  public class RaceConditionSample
  {

    // kilit objesi tanımlamak
    private static readonly object lockObj = new object();

    // Lock Object
    // Interlocked (Atomic operations)

    // Mutex (Thread)
    // Monitor (Thread)
    // Semaphore (Async Task)
    // Concurency Collections (ConcurrentDictionary, ConcurrentQueue, ConcurentBag etc.)


    // race conditio durumu simüle delim
    // aynı process içerisinde çalışırken lockObject mekanizması ile race condition durumunu engelleyebiliriz.
    // Sayısal değer değil de bir nesne üzerinde çalışırken lock mekanizması kullanmak daha mantıklı olabilir.
    public static void RaceConditionDemo()
    {
      const int iterasyon = 1000000;

      // 1) KİLİTSİZ: sayac++ atomik değildir (oku-artır-yaz). İki thread araya girince artışlar kaybolur.
      int kilitsizSayac = 0;

      Thread u1 = new Thread(() => { for (int i = 0; i < iterasyon; i++) kilitsizSayac++; });
      Thread u2 = new Thread(() => { for (int i = 0; i < iterasyon; i++) kilitsizSayac++; });
      u1.Start();
      u2.Start();
      u1.Join();
      u2.Join();

      Console.WriteLine($"[Kilitsiz] Beklenen: {iterasyon * 2}, Gerçek: {kilitsizSayac} (genelde beklenenden küçük)");

      // 2) KİLİTLİ: lock ile aynı anda sadece tek thread sayac'a erişir.
      int sayac = 0;

      Thread t1 = new Thread(() =>
      {
        for (int i = 0; i < iterasyon; i++)
        {
          lock (lockObj) // sayac değişkenine eriş
            sayac++;
        }
      });

      Thread t2 = new Thread(() =>
      {
        for (int i = 0; i < iterasyon; i++)
        {
          lock (lockObj) // sayac değişkenine eriş
            sayac++;
        }
      });


      t1.Start();
      t2.Start();

      t1.Join();
      t2.Join();

      Console.WriteLine($"[Lock] Beklenen: {iterasyon}, Gerçek: {sayac}");
    }


    // Add, Increment, Decrement, Exchange, CompareExchange gibi methodlar mevcuttur. Bu methodlar, bir değişken üzerinde atomik işlemler gerçekleştirir ve
    // lock kıyasla daha hızlıdır. Sayısal tiplerde (int, long...) Add/Increment çalışır; Exchange<T>/CompareExchange<T> referans tiplerle de kullanılabilir.

    public static void InterlockDemo()
    {

      int sayac = 0;

      Thread t1 = new Thread(() =>
      {
        for (int i = 0; i < 1000000; i++)
        {
          Interlocked.Add(ref sayac, 1);
        }
      });


      Thread t2 = new Thread(() =>
      {
        for (int i = 0; i < 1000000; i++)
        {
          Interlocked.Add(ref sayac, 1);
        }
      });

      t1.Start();
      t2.Start();
      t1.Join();
      t2.Join();

      Console.WriteLine($"Sayac degeri: {sayac}"); // Beklenen her zaman 2.000.000

    }


    // Monitor 
    // Monitor sınıfı, lock mekanizmasının daha esnek bir versiyonudur. Monitor.Enter ve Monitor.Exit methodları ile kilitleme işlemi yapılır. Monitor, lock mekanizmasına göre daha fazla kontrol sağlar ve bazı durumlarda performans avantajı sunabilir.
    // TryEnter methodu ile zaman aşımı tanımlayarak deadlock önlenebilir. long running işlemler için daha kullanışlı olabilir. 


    // lock ile kitlemek yerine Monitor Enter ve Exit kullanılabilir. 
    public static void MonitorDemo()
    {
      int sayac = 0;

      void Calis()
      {
        for (int i = 0; i < 5; i++)
        {
          Monitor.Enter(lockObj); // kilit alma işlemi
          try
          {
            sayac++;
          }
          finally
          {
            Monitor.Exit(lockObj); // kilit bırakma işlemi
          }
        }
      }

      Thread t1 = new Thread(Calis);
      Thread t2 = new Thread(Calis);
      t1.Start();
      t2.Start();
      t1.Join();
      t2.Join();

      Console.WriteLine($"[Monitor] Beklenen: {sayac}");
    }


    public static void MonitorDemo2()
    {
      int sayac = 0;
      int atlanan = 0;

      object monitorObject = new object();

      void Calis()
      {
        for (int i = 0; i < 5; i++)
        {
          bool lockTaken = false;
          try
          {
            // 1000 ms boyunca kilit almaya çalış, alınamazsa bu iterasyonu atla
            Monitor.TryEnter(monitorObject, 1000, ref lockTaken);

            if (lockTaken) // kilit tutuluyor mu ? 
            {
              sayac++;
            }
            else
            {
              Interlocked.Increment(ref atlanan);
            }
          }
          finally
          {
            // Sadece kilit alındıysa bırak, yoksa SynchronizationLockException fırlar.
            if (lockTaken)
              Monitor.Exit(monitorObject);
          }
        }
      }

      // Atlamayı göstermek için: 1. thread kilidi 1,5 sn tutar, 2. thread 1 sn timeout ile almaya çalştığından 1 thread atlatır. 
      Thread tutan = new Thread(() =>
      {
        Monitor.Enter(monitorObject);
        try
        {
          Thread.Sleep(1500); // kilidi 1,5 sn boyunca tut
        }
        finally
        {
          Monitor.Exit(monitorObject);
        }
      });

      Thread alan = new Thread(Calis);

      tutan.Start();
      alan.Start();
      tutan.Join();
      alan.Join();

      Console.WriteLine($"[Monitor TryEnter] sayaç {sayac}, atlanan {atlanan}");
    }



    // Mutex -> Mutual Exclusion: Mutex, birden fazla thread veya process arasında paylaşılan bir kaynağa erişimi kontrol etmek için kullanılan bir senkronizasyon aracıdır.

    /* Nerede kullanılır?
      - Birden fazla program aynı log, config veya veritabanı dosyasına yazacaksa
      - Uygulamanın aynı anda sadece bir kopyasının çalışmasını istiyorsanız(single - instance)
      - Paylaşılan donanım gibi tek kullanımlık bir kaynak varsa 
    */

    //  Nerede gerekmez? Tek bir programın kendi thread'leri arasında işlem yapıyorsanız lock yeterlidir. Mutex, sadece process'ler arasında koordinasyon gerektiğinde kullanılır.


    /* Thread affinity, bir kaynağın veya işlemin belirli bir thread'e bağlı olması demektir. Kilidi alan thread'in aynı thread olarak bırakması gerekir, başka bir thread bırakamaz.

       Örnek: Mutex'te WaitOne() ile kilidi alan thread, ReleaseMutex() de aynı thread'den çağrılmalıdır. Başka bir thread çağırırsa hata alırsınız.

       Bu, async/await ile ilgili bir sorun doğurur: await sonrası kod farklı bir thread'de devam edebilir. Bu yüzden await arasında mutex alıp bırakmaya çalışmak hata verir. Async kodda SemaphoreSlim kullanılmasının nedeni de budur, çünkü o thread'e bağlı değildir.

     */

    public static void MutexDemo()
    {
      const int threadSayisi = 10;
      const int iterasyon = 100; // her thread sayacı 100 kez artırır -> beklenen toplam 10.000

      // 1) KİLİTSİZ: Senkronizasyon olmadan paylaşılan sayaç artırılırsa artışlar kaybolur.
      // Mutex'in neden gerektiğini görmek için önce bu sonucu izleyin (genelde beklenenden küçük çıkar).
      int kilitsizSayac = 0;
      Thread[] kilitsizThreadler = new Thread[threadSayisi];
      for (int i = 0; i < threadSayisi; i++)
      {
        kilitsizThreadler[i] = new Thread(() =>
        {
          for (int j = 0; j < iterasyon; j++)
            kilitsizSayac++;
        });
      }
      foreach (var t in kilitsizThreadler) t.Start();
      foreach (var t in kilitsizThreadler) t.Join();


      Console.WriteLine($"[Kilitsiz] Beklenen: {threadSayisi * iterasyon}, Gerçek: {kilitsizSayac}");

      // 2) ÖNEMLİ: ADSIZ MUTEX: Adsız kullanımda Bu mutex yalnızca bu process içinde geçerlidir.
      // Aynı işi lock ile de yapardı; Mutex burada sadece mantığı gösterir. Adsız mutex lock ile de yapılabilir.

      // Senaryo: Diyelim ki aynı exe'yi iki kez çalıştırdınız ve ikisi de aynı log.txt dosyasına yazıyor.
      // Her process bu kodu çalıştırır

      int mutexSayac = 0;
      using Mutex m = new Mutex(initiallyOwned: false, name: "LogDosyasiKilidi"); // aynı isim aynı kilit

      Thread[] mutexThreadler = new Thread[threadSayisi];
      for (int i = 0; i < threadSayisi; i++)
      {
        mutexThreadler[i] = new Thread(() =>
        {
          for (int j = 0; j < iterasyon; j++)
          {
            m.WaitOne();              // Sıram gelene kadar bekle
            try
            {
              // Kritik bölge: dosyaya aynı anda sadece bir process yazabilir
              File.AppendAllText("log.txt", $"{DateTime.Now} - PID {Environment.ProcessId}\n");
              mutexSayac++;
            }
            finally
            {
              m.ReleaseMutex();       // Sıradakine geçiş izni ver
            }
          }
        });
      }
      foreach (var t in mutexThreadler) t.Start();
      foreach (var t in mutexThreadler) t.Join();

      Console.WriteLine($"[Mutex] Beklenen: {threadSayisi * iterasyon}, Gerçek: {mutexSayac}");

    }



    // Asenkron (await'li) operasyonlarda eşzamanlılığı sınırlamak için SemaphoreSlim kullanılır.
    // Uyarı: lock içinde await yapılamaz, SemaphoreSlim.WaitAsync yapılabilir.
    // N görev aynı anda en fazla belirli sayıda işlem yapabilir. Mutex (1,1) OS düzeyindeydi ve process'ler arası çalışırdı.
    // SemaphoreSlim sadece process için çalışır hafif ve hızlıdır.
    // Senaryo olarak: Bir api endpointe aynı anda en fazla 4 istek yapılabilsin. (Throttling)

    public static async Task SemaphoreSlimSample()
    {

      // Aynı anda en fazla 4 görev. (maxCount verilmezse sınırsız; initialCount = başlangıçtaki boş slot sayısı)
      using SemaphoreSlim semaphoreSlim = new SemaphoreSlim(20);

      // 20 masalı bir restoran düşünelim: ilk 20 müşteri direkt oturur,
      // kalan 80 müşteri bir masa boşalana kadar WaitAsync'te (thread'i bloklamadan) bekler.

      Task[] gorevler = Enumerable.Range(0, 100).Select(async i =>
      {

        await semaphoreSlim.WaitAsync(); // slot boşalana kadar beklet, kilit mekanizması
        Console.WriteLine($"Müşteri masaya oturdu, Kalan Slot: {semaphoreSlim.CurrentCount}");

        try
        {

          await Task.Delay(30);

        }
        finally
        {

          semaphoreSlim.Release(); // slotu serberst bırak.
          Console.WriteLine($"Müşteri kalktı, yeni bir müşteri oturabilir, Kalan Slot: {semaphoreSlim.CurrentCount}");
        }


      }).ToArray();


      await Task.WhenAll(gorevler);

    }


    // ReaderWriterLockSlim: Bu sınıf, bir kaynağa aynı anda birden fazla okuyucunun erişmesine izin verirken, yazıcıların tek başına erişmesini sağlar. Bu, özellikle okuma işlemlerinin yazma işlemlerinden daha sık olduğu durumlarda performansı artırabilir. ReaderWriterLockSlim, lock mekanizmasına göre daha karmaşık bir yapıya sahiptir ve dikkatli kullanılmalıdır. Thread üzerinde çalışma yapıyoruz. 

    // Senaryo: Mevcut 5 kişi bir dosyayı okuyor. (Read Lock aktif)
    // içlerinden biri dosya üzerinden düzenleme yapmak istiyor. (EnterWriteLock)
    // 6. kişi dışarıdan bu durumda okuma için geldiğinde, yazma işlemi bitene kadar beklemesi gerekir. (Read Lock bekletilir)
    // Yazma işlemininde yapılabilmesi için 5 kişinin okuma işlemini bitirmesi gerekir. (Write Lock bekletilir)
    // Daha çok okumanın olduğu ortak paylaşılan kaynaklarda, ReaderWriterLockSlim kullanmak performans açısından daha avantajlı olabilir. Ancak, yazma işlemlerinin sık olduğu durumlarda, bu sınıfın kullanımı performans sorunlarına yol açabilir. Bu nedenle, kullanım senaryosuna göre doğru senkronizasyon aracını seçmek önemlidir.

    // Uyarı: Tüm readerlar toplamda 200ms içerisinde paylaşımlı okuma yapıyorlar.

    public static void ReaderWriterLockSlimSample()
    {
      Stopwatch sp = Stopwatch.StartNew();


      using ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
      // Okuma işlemi
      Thread[] readerThreads = Enumerable.Range(0, 10).Select(item => new Thread(() =>
      {
        rwLock.EnterReadLock(); // Okuma kilidini al
        try
        {
          Console.WriteLine("Okuma işlemi başladı.");
          Thread.Sleep(200); // Okuma işlemi simülasyonu
          Console.WriteLine("Okuma işlemi tamamlandı.");
        }
        finally
        {
          rwLock.ExitReadLock(); // Okuma kilidini bırak
        }

      })).ToArray();

      // Yazma işlemi
      Thread writerThread = new Thread(() =>
      {
        rwLock.EnterWriteLock(); // Yazma kilidini al
        try
        {
          Console.WriteLine("Yazma işlemi başladı.");
          Thread.Sleep(3000); // Yazma işlemi simülasyonu
          Console.WriteLine("Yazma işlemi tamamlandı.");
        }
        finally
        {
          rwLock.ExitWriteLock(); // Yazma kilidini bırak
        }
      });


      // Yanlış kullanım
      // Önce hepsini başlat (Join'i hemen çağırırsak okuyucular sıralı çalışır ve paylaşımlı okuma görünmez).
      foreach (var readerThread in readerThreads)
      {
        readerThread.Start();
        // readerThread.Join();
      }

      writerThread.Start();

      // Doğru kullanım
      //foreach (var readerThread in readerThreads)
      //  readerThread.Join();

      writerThread.Join();

      // Okuyucular paralel çalışır (~200 ms), writer tek başına (3000 ms). Toplam ≈ 3200 ms (sıralı olsaydı 5000 ms).
      Console.WriteLine($"Toplam süre: {sp.ElapsedMilliseconds} ms");

    }


  }
}
