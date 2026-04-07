using System;
using Verse;

namespace TeleCore.Loader;

public abstract class DefInjectBase : IDisposable
{
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void OnThingDefInject(ThingDef thingDef) { }

    public virtual void OnPawnInject(ThingDef pawnDef) { }

    public virtual void OnBuildableDefInject(BuildableDef def) { }

    public virtual bool AcceptsSpecial(Def def) { return true; }

    public virtual void OnDefSpecialInjected(Def def) { }

    public virtual void Dispose(bool disposing) { }
}