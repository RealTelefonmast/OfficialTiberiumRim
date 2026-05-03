using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class ObjectiveResearchDef : ObjectiveDef
{
    public List<ThingDef> dependencies = new();
}

public class Objective_TResearch : Objective
{
}