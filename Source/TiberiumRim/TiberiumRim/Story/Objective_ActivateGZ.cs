using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StoryFramework;
using Verse;

namespace TiberiumRim
{
    public class Objective_ActivateGZ : Objective
    {
        public Objective_ActivateGZ() { }

        public override void OnFinish()
        {
            Find.World.GetComponent<WorldComponent_Tiberium>().GroundZero.producer.researchDone = true;
        }
    }
}
