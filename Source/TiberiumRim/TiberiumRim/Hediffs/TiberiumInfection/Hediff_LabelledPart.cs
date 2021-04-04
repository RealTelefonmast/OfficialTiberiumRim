using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Hediff_LabelledPart : HediffWithComps
    {
        public override string LabelBase => def.label.Formatted(this.Part.Label);
    }
}
