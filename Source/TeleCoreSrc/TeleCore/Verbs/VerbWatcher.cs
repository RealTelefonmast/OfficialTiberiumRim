using System.Collections.Generic;
using Verse;

namespace TeleCore.Verbs;

public static class VerbWatcher
{
    private static Dictionary<Verb, TeleVerbAttacher> _watched;
    
    static VerbWatcher()
    {
        _watched = new Dictionary<Verb, TeleVerbAttacher>();
    }
    
    internal static void Notify_NewVerb(Verb verb, VerbTracker tracker)
    {
        if (_watched.ContainsKey(verb) || verb.verbProps is not VerbProperties_Tele) return;
        var attacher = new TeleVerbAttacher(tracker.directOwner, verb);
        _watched.Add(verb, attacher);
    }
    
    internal static bool HasAttacher(Verb verb)
    {
        return _watched.ContainsKey(verb);
    }
    
    internal static TeleVerbAttacher? GetAttacher(Verb verb)
    {
        return _watched.GetValueOrDefault(verb);
    }
}