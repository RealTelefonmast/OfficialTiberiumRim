using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class DiscoveryDef : Def
    {
        public WikiEntryDef wikiEntry;

        public void Discover()
        {
            TRUtils.DiscoveryTable().Discover(this);
        }
    }
}
