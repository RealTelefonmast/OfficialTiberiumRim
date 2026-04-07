using Verse;

namespace TeleCore.Events;

public struct FXEffecterSpawnedEventArgs
{
    public string effecterTag;
    public FleckDef fleckDef;
    public Mote mote;
}