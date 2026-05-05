// Preserved from TiberiumRim/Research/Wiki/WikiEntryDef.cs

using System.Collections.Generic;
using Verse;

namespace TeleCore.Research.Defs;

public class WikiEntryDef : Def
{
    public List<string> imagePaths;

    //public Type entryPageWorker = typeof(WikiEntryPage);
    public ThingDef wikiThing;
}
