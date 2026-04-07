namespace TeleCore.FlowCore;

public interface IContainerLeaker
{
    bool ShouldLeak { get; }
}