using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using StoryFramework;

namespace TiberiumRim
{
    public class ObjectiveResearchDef : ObjectiveDef
    {
        public List<ThingDef> dependencies = new List<ThingDef>();
    }

    public class Objective_TResearch : Objective
    {

    }
}
