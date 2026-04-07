using Verse;

namespace TeleCore.Generics.Container;

public struct FlowValueFilterSettings : IExposable
{
    public bool canReceive = true;
    public bool canStore = true;
    public bool canTransfer = true;

    public FlowValueFilterSettings()
    {
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref canReceive, nameof(canReceive));
        Scribe_Values.Look(ref canStore, nameof(canStore));
        Scribe_Values.Look(ref canTransfer, nameof(canTransfer));
    }
}