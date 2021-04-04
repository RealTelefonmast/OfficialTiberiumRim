using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public interface IResearchCraneTarget
    {
        public Building ResearchCrane { get; }
        public bool ResearchBound { get; }
    }
}
