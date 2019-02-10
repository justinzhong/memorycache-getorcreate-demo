using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Demo
{
    public class Demo
    {
        private readonly static Random _rand = new Random();
        private readonly ConcurrentDictionary<string, Lazy<int>> _concurrentDictionary;
        private readonly Dictionary<string, Lazy<int>> _dictionary;
        private readonly IMemoryCache _memoryCache;

        public Demo(IMemoryCache memoryCache)
        {
            _concurrentDictionary = new ConcurrentDictionary<string, Lazy<int>>();
            _dictionary = new Dictionary<string, Lazy<int>>();
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task Run(int runs)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.WriteLine($"# MemoryCache {runs} x GetOrAdd");
            await RunTest(WithMemoryCache, runs);

            Console.WriteLine("\r\n--\r\n");

            Console.WriteLine($"# ConcurrentDictionary {runs} x GetOrCreate");
            await RunTest(WithConcurrentDictionary, runs);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
        }

        private async Task RunTest(Func<int> scenario, int runs)
        {
            var result = await Task.WhenAll(Enumerable.Range(1, runs).Select(run => Task.Run(() => scenario())));
            var distinctResult = result.Distinct().Select(r => r.ToString()).ToArray();

            Console.WriteLine($"{(distinctResult.Length == 1 ? "consistent output" : "ambiguous output")}: [{string.Join(", ", distinctResult)}]");
        }

        private int WithConcurrentDictionary()
        {
            var factory = new Lazy<int>(() =>
            {
                var rand = _rand.Next();

                Console.WriteLine($"[thread {Thread.CurrentThread.ManagedThreadId}]: Invoked factory with random value: {rand}");

                return rand;
            });

            var ret = _concurrentDictionary.GetOrAdd("key", (entry) =>
            {
                Console.WriteLine($"[thread {Thread.CurrentThread.ManagedThreadId}]: Cache key miss");
                Task.Delay(10);

                return factory;
            });

            Thread.Sleep(5);

            return ret.Value;
        }

        private int WithMemoryCache()
        {
            var factory = new Lazy<int>(() =>
            {
                var rand = _rand.Next();

                Console.WriteLine($"[thread {Thread.CurrentThread.ManagedThreadId}]: Invoked factory with random value: {rand}");

                return rand;
            });

            var ret = _memoryCache.GetOrCreate("key", (entry) =>
            {
                Console.WriteLine($"[thread {Thread.CurrentThread.ManagedThreadId}]: Cache key miss");
                Task.Delay(10);

                return factory;
            });

            Thread.Sleep(5);

            return ret.Value;
        }
    }
}
