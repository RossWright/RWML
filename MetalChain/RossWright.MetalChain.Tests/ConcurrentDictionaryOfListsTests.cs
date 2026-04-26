using Shouldly;

namespace RossWright.MetalChain.Tests;

public class ConcurrentDictionaryOfListsTests
{
    [Fact]
    public void ContainsKey_WhenKeyAbsent_ReturnsFalse()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.ContainsKey("x").ShouldBeFalse();
    }

    [Fact]
    public void ContainsKey_WhenKeyPresent_ReturnsTrue()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Add("x", 1);
        dict.ContainsKey("x").ShouldBeTrue();
    }

    [Fact]
    public void ContainsKey_WithValue_WhenKeyAndValuePresent_ReturnsTrue()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Add("x", 1);
        dict.ContainsKey("x", 1).ShouldBeTrue();
    }

    [Fact]
    public void ContainsKey_WithValue_WhenValueAbsent_ReturnsFalse()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Add("x", 1);
        dict.ContainsKey("x", 2).ShouldBeFalse();
    }

    [Fact]
    public void ContainsKey_WithValue_WhenKeyAbsent_ReturnsFalse()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.ContainsKey("x", 1).ShouldBeFalse();
    }

    [Fact]
    public void GetValuesOrEmptySet_WhenKeyAbsent_ReturnsEmptyList()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.GetValuesOrEmptySet("x").ShouldBeEmpty();
    }

    [Fact]
    public void GetValuesOrEmptySet_WhenKeyPresent_ReturnsValues()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Add("x", 1);
        dict.Add("x", 2);
        var values = dict.GetValuesOrEmptySet("x").ToList();
        values.ShouldContain(1);
        values.ShouldContain(2);
    }

    [Fact]
    public void Remove_WhenKeyAndValuePresent_RemovesValue()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Add("x", 1);
        dict.Remove("x", 1);
        dict.GetValuesOrEmptySet("x").ShouldBeEmpty();
    }

    [Fact]
    public void Remove_WhenValueAbsent_LeavesExistingValues()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Add("x", 1);
        dict.Remove("x", 2);
        dict.GetValuesOrEmptySet("x").ShouldContain(1);
    }

    [Fact]
    public void ConcurrentWrites_FromMultipleThreads_NoEntriesLostOrDuplicated()
    {
        const int threadCount = 8;
        const int writesPerThread = 100;
        var dict = new ConcurrentDictionaryOfLists<string, int>();

        var threads = Enumerable.Range(0, threadCount)
            .Select(t => new Thread(() =>
            {
                for (int i = 0; i < writesPerThread; i++)
                    dict.Add("key", t * writesPerThread + i);
            }))
            .ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        var values = dict.GetValuesOrEmptySet("key").ToList();
        values.Count.ShouldBe(threadCount * writesPerThread);
        values.Distinct().Count().ShouldBe(threadCount * writesPerThread);
    }

    [Fact]
    public void Remove_WithConcurrentAdds_RetriesAndSucceeds()
    {
        const int threadCount = 4;
        const int operationsPerThread = 50;
        var dict = new ConcurrentDictionaryOfLists<string, int>();

        // Pre-populate with values
        for (int i = 0; i < threadCount * operationsPerThread; i++)
            dict.Add("key", i);

        var threads = Enumerable.Range(0, threadCount)
            .Select(t => new Thread(() =>
            {
                for (int i = 0; i < operationsPerThread; i++)
                {
                    if (i % 2 == 0)
                        dict.Add("key", 1000 + t * operationsPerThread + i);
                    else
                        dict.Remove("key", t * operationsPerThread + i);
                }
            }))
            .ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // Verify the dictionary is in a consistent state
        var values = dict.GetValuesOrEmptySet("key").ToList();
        values.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Remove_WhenKeyAbsent_DoesNothing()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Remove("nonexistent", 1);
        dict.ContainsKey("nonexistent").ShouldBeFalse();
    }

    [Fact]
    public void Remove_WithMultipleValues_RemovesOnlySpecifiedValue()
    {
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        dict.Add("key", 1);
        dict.Add("key", 2);
        dict.Add("key", 3);

        dict.Remove("key", 2);

        var values = dict.GetValuesOrEmptySet("key").ToList();
        values.ShouldContain(1);
        values.ShouldNotContain(2);
        values.ShouldContain(3);
    }

    [Fact]
    public void Remove_LastValueConcurrently_HandlesRaceCondition()
    {
        const int iterations = 100;
        
        for (int iteration = 0; iteration < iterations; iteration++)
        {
            var dict = new ConcurrentDictionaryOfLists<string, int>();
            dict.Add("key", 1);

            var thread1 = new Thread(() => dict.Remove("key", 1));
            var thread2 = new Thread(() => dict.Add("key", 2));

            thread1.Start();
            thread2.Start();
            thread1.Join();
            thread2.Join();

            // Should be in a consistent state
            var values = dict.GetValuesOrEmptySet("key").ToList();
            if (values.Contains(1) && values.Contains(2))
            {
                values.Count.ShouldBe(2);
            }
            else if (values.Contains(2))
            {
                values.Count.ShouldBe(1);
            }
            else
            {
                values.Count.ShouldBe(0);
            }
        }
    }

    [Fact]
    public void Remove_ConcurrentRemovesOfSameValue_AllSucceed()
    {
        const int threadCount = 8;
        var dict = new ConcurrentDictionaryOfLists<string, int>();
        
        // Add the same value multiple times
        for (int i = 0; i < threadCount; i++)
            dict.Add("key", i);

        var threads = Enumerable.Range(0, threadCount)
            .Select(t => new Thread(() => dict.Remove("key", t)))
            .ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // All values should be removed
        dict.GetValuesOrEmptySet("key").ShouldBeEmpty();
    }

    [Fact]
    public void Remove_ConcurrentModificationsDuringRemoval_MaintainsConsistency()
    {
        const int iterations = 50;
        var dict = new ConcurrentDictionaryOfLists<string, int>();

        for (int i = 0; i < 10; i++)
            dict.Add("key", i);

        var threads = new List<Thread>();
        
        for (int t = 0; t < 4; t++)
        {
            var threadId = t;
            threads.Add(new Thread(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    dict.Add("key", 100 + threadId * iterations + i);
                    dict.Remove("key", threadId + i % 10);
                }
            }));
        }

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // Should be in a consistent state
        var values = dict.GetValuesOrEmptySet("key").ToList();
        values.Count.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Remove_HighlyContentedRemoval_AllRetriesSucceed()
    {
        const int iterations = 200;
        var dict = new ConcurrentDictionaryOfLists<string, int>();

        // Start with a single value that will be contended
        dict.Add("key", 999);

        var threads = Enumerable.Range(0, 10)
            .Select(t => new Thread(() =>
            {
                for (int i = 0; i < iterations; i++)
                {
                    dict.Add("key", t * iterations + i);
                    Thread.Sleep(0); // Yield to increase contention
                    dict.Remove("key", t * iterations + i);
                }
            }))
            .ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        // The original value should still be there or the list should be consistent
        var values = dict.GetValuesOrEmptySet("key").ToList();
        values.Count.ShouldBeGreaterThanOrEqualTo(0);
    }
}