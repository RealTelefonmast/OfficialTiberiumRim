using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class WikiEntryDef : Def
    {
        //public Type entryPageWorker = typeof(WikiEntryPage);
        public ThingDef wikiThing;
        public List<string> imagePaths;
    }
}
