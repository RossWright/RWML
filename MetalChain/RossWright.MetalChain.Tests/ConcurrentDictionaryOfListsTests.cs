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
}