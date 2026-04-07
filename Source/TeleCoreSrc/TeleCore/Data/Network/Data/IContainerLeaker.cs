namespace TeleCore.Network;

public interface IContainerLeaker
{
    bool ShouldLeak { get; }
}