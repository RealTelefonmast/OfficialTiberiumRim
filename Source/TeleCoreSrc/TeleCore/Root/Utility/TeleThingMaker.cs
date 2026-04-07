using System;
using RimWorld;
using TeleCore.Logging;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Utility;

public static class TeleThingMaker
{
    public static TThing MakeThingGeneric<TThing>(ThingDef def, ThingDef stuff = null)
        where TThing : Thing
    {
        if (stuff != null && !stuff.IsStuff)
        {
            TLog.Error(string.Concat("MakeThing error: Tried to make ", def, " from ", stuff,
                " which is not a stuff. Assigning default."));
            stuff = GenStuff.DefaultStuffFor(def);
        }

        if (def.MadeFromStuff && stuff == null)
        {
            Log.Error("MakeThing error: " + def + " is madeFromStuff but stuff=null. Assigning default.");
            stuff = GenStuff.DefaultStuffFor(def);
        }

        if (!def.MadeFromStuff && stuff != null)
        {
            Log.Error(string.Concat("MakeThing error: ", def, " is not madeFromStuff but stuff=", stuff,
                ". Setting to null."));
            stuff = null;
        }

        var thing = (TThing)Activator.CreateInstance(typeof(TThing));
        thing.def = def;
        thing.SetStuffDirect(stuff);
        thing.PostMake();
        thing.PostPostMake();
        return thing;
    }
}