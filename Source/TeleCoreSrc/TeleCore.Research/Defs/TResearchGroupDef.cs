using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TeleCore.Research.Defs;

public class TResearchGroupDef : Def
{
    public int priority = 0;
    public List<TResearchDef> researchProjects;

    //TODO: Fallback value?
    public List<TResearchDef> ActiveProjects => researchProjects.NullOrEmpty()
        ? null
        : researchProjects.Where(t => t.RequisitesComplete).ToList();

    public bool IsVisible =>
        (!IsFinished && !ActiveProjects.NullOrEmpty()) || (IsFinished && !TResearchManager.hideGroups);

    public bool IsFinished => researchProjects.NullOrEmpty() || researchProjects.All(r => r.IsFinished);

    public bool HasUnseenProjects => ActiveProjects.Any(t => !t.HasBeenSeen);
}