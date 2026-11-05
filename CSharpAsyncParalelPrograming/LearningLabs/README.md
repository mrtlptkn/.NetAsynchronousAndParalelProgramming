# LearningLabs

.NET'te thread, async/Task, paralel programlama ve senkronizasyon konularının çalıştırılabilir örnekleri.
Her örnek `LearningLabsController` içinde bir `POST /api/LearningLabs/{route}` endpoint'ine bağlıdır (örnek istekler: `../CSharpAsyncParalelPrograming.http`). Çıktıları API konsolunda izleyin.

## Dosyalar

| Dosya | İçerik |
|---|---|
| `ThreadSample.cs` | Thread oluşturma, parametre, foreground/background, Join, öncelik, closure tuzağı |
| `TaskSample.cs` | async/await, iptal, hata yönetimi, `WhenAll`, `WhenAny`, `ValueTask` |
| `TaskService.cs` | DI ile kullanılan servis: iptal destekli async metot, sync-over-async |
| `UserResponse.cs` | Servisin örnek DTO'su |
| `ParallelProgrammingSample.cs` | `Parallel.For/ForEach/Invoke/ForAsync`, thread-local state, PLINQ |
| `RaceConditionSample.cs` | Race condition, `lock`, `Interlocked`, `Monitor`, `Mutex`, `SemaphoreSlim`, `ReaderWriterLockSlim`, concurrent koleksiyonlar |

## Kavram → Lab → Endpoint

| Kavram | Metot | Route |
|---|---|---|
| Thread | `ThreadSample.BasicThreadSample` | `basicThread` |
| Thread + parametre | `ThreadWithParameter` | `threadWithParameter` |
| Foreground / background | `ForegroundVsBackgroundThread` | `foregroundVsBackgroundThread` |
| Çoklu thread + Join | `MultiThreadsWithJoin` | `multiThreadsWithJoin` |
| async/await | `TaskSample.BasicTaskSample` | `basicTaskSample` |
| Yanlış async (fire-and-forget) | `BasicTask2Sample` | `basicTaskSample2` |
| İptal (`CancellationToken`) | `TaskCancelSample`, `TaskService.GetUsersAsync(ct)` | `taskCancelSample`, `ClientCancelRequest` |
| Hata yönetimi | `TaskExceptionSample` | `taskExceptionSample` |
| Hepsini bekle / ilk biteni al | `TaskWhenAll`, `TaskWhenAny` | `TaskWhenAll`, `TaskWhenAny` |
| CPU-bound paralel döngü | `ParallelProgrammingSample.BasicParallelFor` | `BasicParallelFor` |
| `Parallel.ForEach` + iptal | `ParallelForeachBasis` | `ParalelForeachBasis` |
| Thread-local state | `ParallelForWithThreadLocalState` | `ParalelForWithThreadLocalState` |
| `Parallel.Invoke` | `ParallelInvokeSample` | `ParalelInvokeSample` |
| Async paralel döngü | `ParallelForAsyncSample` | `ParalelForAsyncSample` |
| PLINQ | `PLINQSample` | `PLINQSample` |
| Race condition + `lock` | `RaceConditionSample.RaceConditionDemo` | `RaceConditionDemo` |
| `Monitor.TryEnter` | `MonitorDemo2` | `MonitorDemo2` |
| `Mutex` | `MutexDemo` | `MutexDemo` |
| `SemaphoreSlim` | `SemaphoreSlimSample` | `SemaphoreSlimSample` |
| `ReaderWriterLockSlim` | `ReaderWriterLockSlimSample` | `ReaderWriterLockSlimSample` |
| `ConcurrentDictionary` / `ConcurrentBag` | `ConcurrentDictionarySample`, `ConcurrentBagSample` | `ConcurrentDictionarySample`, `ConcurentBag` |
| Thread-safe olmayan `Dictionary` | `NormalDictionarySample` | `NormalDictionarySample` |

> Route adları geriye dönük uyumluluk için eski yazımlarıyla (`Paralel…`, `Concurent…`) bırakıldı.

## Ne zaman hangisi?

| İhtiyaç | Kullan |
|---|---|
| I/O bekleme (HTTP, DB, dosya) | `async/await` (thread bloklanmaz; `Task.Run` gerekmez) |
| CPU yoğun iş, çok veri | `Parallel.For/ForEach` veya PLINQ (küçük veride sıralı daha hızlıdır, ölçün) |
| Birden çok async işi aynı anda bekle | `Task.WhenAll` |
| İlk biteni al / timeout | `Task.WhenAny` |
| Paralel döngüde async iş | `Parallel.ForEachAsync` / `ForAsync` (asla `async` lambda ile `Parallel.For`) |
| Basit sayaç | `Interlocked` |
| Kritik bölge (senkron kod) | `lock` (.NET 9: `System.Threading.Lock`) |
| Kritik bölge içinde `await` / eşzamanlılık sınırı | `SemaphoreSlim` |
| Process'ler arası kilit | Adlandırılmış `Mutex` / `Semaphore` |
| Çok okuma, az yazma | `ReaderWriterLockSlim` |
| Paylaşılan anahtar-değer | `ConcurrentDictionary` |
| Üretici-tüketici | `ConcurrentQueue`, `BlockingCollection`, `Channel<T>` |

## Sık yapılan hatalar

- `Task` / `Task.Delay` await etmemek (bekleme olmaz, hata kaybolur).
- `CancellationToken`'ı alt çağrılara (`Task.Delay`, `HttpClient`, EF) iletmemek.
- `.Result` / `.Wait()` / `GetAwaiter().GetResult()` ile bloklamak (deadlock veya thread pool starvation).
- Döngü değişkenini thread/lambda içinde doğrudan yakalamak (`int idx = i;` kullanın).
- `Parallel.For` içinde paylaşılan değişkene kilitsiz yazmak; `localFinally` da eşzamanlı çalışır.
- Thread-safe olmayan `List`/`Dictionary`'ye paralel yazmak.
