namespace RossWright;

public class OnDisposeTests
{
    [Fact] public void Dispose_ActionCalledExactlyOnce()
    {
        int callCount = 0;
        var disposable = new OnDispose(() => callCount++);
        disposable.Dispose();
        callCount.ShouldBe(1);
    }

    [Fact] public void Dispose_ActionNotCalledBeforeDispose()
    {
        int callCount = 0;
        _ = new OnDispose(() => callCount++);
        callCount.ShouldBe(0);
    }

    [Fact] public void Dispose_SecondCallIsNoOp()
    {
        int callCount = 0;
        var disposable = new OnDispose(() => callCount++);
        disposable.Dispose();
        disposable.Dispose();
        callCount.ShouldBe(1);
    }

    [Fact] public async Task OnDisposeAsync_ActionCalledExactlyOnce()
    {
        int callCount = 0;
        var disposable = new OnDisposeAsync(() => { callCount++; return Task.CompletedTask; });
        await disposable.DisposeAsync();
        callCount.ShouldBe(1);
    }

    [Fact] public async Task OnDisposeAsync_SecondCallIsNoOp()
    {
        int callCount = 0;
        var disposable = new OnDisposeAsync(() => { callCount++; return Task.CompletedTask; });
        await disposable.DisposeAsync();
        await disposable.DisposeAsync();
        callCount.ShouldBe(1);
    }
}
