using System.ComponentModel;

namespace TeleCore.Types.Interfaces;

public interface IContainerLeaker
{
    bool ShouldLeak { get; }

    public Container { get; }
}