namespace RossWright.MetalShout;

public interface IPushConnectionObserver
{
    Task OnConnected(Guid? userId, bool isFirstConnection);
    Task OnDisconnected(Guid? userId, Exception? exception, bool wasLastConnection);
}
