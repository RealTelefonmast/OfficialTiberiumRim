using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TRGroupDef : Def
    {
        [Unsaved] 
        private TRGroupDef parentGroup;

        public List<TRGroupDef> subGroups;

        public TRGroupDef ParentGroup => parentGroup;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            foreach (var groupDef in DefDatabase<TRGroupDef>.AllDefs)
            {
                if(groupDef.subGroups is null) continue;
                if (groupDef.subGroups.Contains(this))
                {
                    parentGroup = groupDef;
                    break;
                }
            }
        }
    }
}
