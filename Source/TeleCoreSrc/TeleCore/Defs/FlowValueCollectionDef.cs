using System.Collections.Generic;
using Verse;

namespace TeleCore.Defs;

public class FlowValueCollectionDef : Def
{
    [field: Unsaved]
    public List<FlowValueDef> ValueDefs { get; } = new();

    public void Notify_ResolvedFlowValueDef(FlowValueDef def)
    {
        ValueDefs.Add(def);
    }

    // TODO: Obsolete?
    // public override void ResolveReferences()
    // {
    //     foreach (var valueDef in ValueDefs)
    //     {
    //         valueDef.collectionDef = this;
    //     }
    // }
}