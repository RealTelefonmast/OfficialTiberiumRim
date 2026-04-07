using System.Collections.Generic;
using Verse;

namespace TeleCore.Wiki
{
    public class WikiEntryDef : Def
    {
        //public Type entryPageWorker = typeof(WikiEntryPage);
        public ThingDef wikiThing;
        public List<string> imagePaths;
    }
}
