namespace RossWright.MetalCommand.Tests;

public class SpinnerTests
{
    [Fact]
    public void Width_IsOne()
    {
        new Spinner().Width.ShouldBe(1);
    }

    [Fact]
    public void CyclesFourDistinctCharacters_AndFifthMatchesFirst()
    {
        var spinner = new Spinner();

        var results = Enumerable.Range(0, 5)
            .Select(_ => spinner.Output(0))
            .ToArray();

        // First four outputs should all be distinct.
        results.Take(4).Distinct().Count().ShouldBe(4);

        // Fifth output should cycle back to the first.
        results[4].ShouldBe(results[0]);
    }
}
