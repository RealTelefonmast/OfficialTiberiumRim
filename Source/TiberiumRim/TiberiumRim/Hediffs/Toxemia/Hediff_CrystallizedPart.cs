using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class Hediff_CrystallizedPart : Hediff_MissingPart
    {
        public override string LabelBase => def.label;

        public override string LabelInBrackets => null;
    }
}
