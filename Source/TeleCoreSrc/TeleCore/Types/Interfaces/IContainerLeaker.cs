using System.ComponentModel;

namespace TeleCore.Unsorted;

public interface IContainerLeaker
{
    bool ShouldLeak { get; }

    public Container { get; }
}