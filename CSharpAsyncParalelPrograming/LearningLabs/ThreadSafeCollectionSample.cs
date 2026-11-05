using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSharpAsyncParalelPrograming.LearningLabs
{
  public static class ThreadSafeCollectionSample
  {


    // Thread Safe Collections
    // Key value cinsinden değer tutacaksak, burada concurent dictionary tercih edebiliriz. Buda sırasız çalışır.
    // Sıralı çalışmak isterseniz ise blocking collection, hem kapasite kontrolü yapar. Hemde ekleme çıkarma işlemlerinde sıralı ve kilit mekanizmalı çalışır.
    public static void ConcurrentDictionarySample()
    {
      Stopwatch sp = Stopwatch.StartNew();
      ConcurrentDictionary<int, string> concurrentDictionary = new ConcurrentDictionary<int, string>();


      Parallel.For(0, 100000, i =>
      {
        concurrentDictionary.TryAdd(i, i.ToString());

        if (concurrentDictionary.TryGetValue(i, out var value))
        {
          if (i == 10)
            Console.WriteLine($"Key:{value}");
        }

        concurrentDictionary.TryRemove(100, out _);
        concurrentDictionary.TryRemove(101, out _);

      });

      sp.Stop();

      // Not: Remove(100/101) ile TryAdd(100/101) birbiriyle yarışır. Silme eklemeden önce çalışırsa anahtar sonradan eklenir.
      // Bu yüzden sonuç deterministik değildir: 99998, 99999 veya 100000 olabilir.
      // GetOrAdd/AddOrUpdate: valueFactory birden fazla kez çalışabilir, pahalı/yan etkili işler için Lazy<T> kullanın.
      Console.WriteLine($"ConcurrentDictionary'de kalan eleman sayısı: {concurrentDictionary.Count}");
      Console.WriteLine($"Tahmini süre: {sp.ElapsedMilliseconds}"); // ConcurrentDictionary'de kalan eleman sayısı
    }


    // Not: Sıralama önemli değilse kullanılabilir (sırasız bir "torba"). Ancak ConcurrentBag dar bir senaryo içindir:
    // her thread kendi eklediği öğeleri tüketiyorsa (thread-local liste) hızlıdır. Count/enumerate pahalıdır.
    // Genel üretici-tüketici için ConcurrentQueue<T> veya Channel<T>, sıralı + kapasite sınırlı için BlockingCollection<T> (varsayılan FIFO) tercih edilir.
    public static void ConcurrentBagSample()
    {
      Stopwatch sp = Stopwatch.StartNew();
      ConcurrentBag<string> bags = new();


      Parallel.For(0, 100000, i =>
      {
        bags.Add(i.ToString());

        if (bags.TryPeek(out var ilk))
        {
          if (i == 10)
            Console.WriteLine($"Peek: {ilk}");
        }

      });

      bags.TryTake(out _);
      bags.TryTake(out _);


      sp.Stop();

      Console.WriteLine($"ConcurrentBag'de kalan eleman sayısı: {bags.Count}"); // 99998
      Console.WriteLine($"Tahmini süre: {sp.ElapsedMilliseconds}");

    }

    public static void NormalDictionarySample()
    {
      // System.ArgumentException: 'Destination array was not long enough. Check the destination index, length, and the array's lower bounds. (Parameter 'destinationArray')'
      // Thread Safe olmayan bir listeyele ParalelFor içerisinde çalışmaya çalışıyorsun.
      Dictionary<int, string> dictionary = new Dictionary<int, string>();

      Parallel.For(0, 100000, i =>
      {
        dictionary.TryAdd(i, i.ToString());

        if (dictionary.TryGetValue(i, out var value))
        {
          if (i == 10)
            Console.WriteLine($"Key: {value}");
        }

        dictionary.Remove(100, out _);
        dictionary.Remove(101, out _);

      });

      Console.WriteLine($"Dictionary'de kalan eleman sayısı: {dictionary.Count}"); // ConcurrentDictionary'de kalan eleman sayısı

    }

  }
}
