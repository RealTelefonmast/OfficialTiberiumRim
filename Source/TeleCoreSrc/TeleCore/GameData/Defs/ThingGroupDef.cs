using System.Collections.Generic;
using Verse;

namespace TeleCore.GameData.Defs;

public class ThingGroupDef : Def
{
    public List<ThingGroupDef> subGroups;
    public ThingGroupDef parentGroup;

    public override void ResolveReferences()
    {
        base.ResolveReferences();
        foreach (var groupDef in DefDatabase<ThingGroupDef>.AllDefs)
        {
            if (parentGroup != null)
            {
                if (groupDef == parentGroup)
                {
                    groupDef.subGroups ??= new List<ThingGroupDef>();
                    groupDef.subGroups.Add(this);
                    break;
                }

                continue;
            }

            //
            if (groupDef.subGroups is null) continue;
            if (groupDef.subGroups.Contains(this))
            {
                parentGroup = groupDef;
                break;
            }
        }
    }
}