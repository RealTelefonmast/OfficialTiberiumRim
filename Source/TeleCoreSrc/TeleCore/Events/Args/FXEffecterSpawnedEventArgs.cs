using Verse;

namespace TeleCore.Events.Args;

public struct FXEffecterSpawnedEventArgs
{
    public string effecterTag;
    public FleckDef fleckDef;
    public Mote mote;
}