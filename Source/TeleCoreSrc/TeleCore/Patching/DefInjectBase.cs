using System;
using Verse;

namespace TeleCore.Patching;

public interface IDefInjection
{
    public void OnThingDefInject(ThingDef thingDef);
    public void OnPawnInject(ThingDef pawnDef);
    public void OnBuildableDefInject(BuildableDef def);
    public bool AcceptsSpecial(Def def);
    public void OnDefSpecialInjected(Def def);
}

[Obsolete("This class will be removed in a future version, please use IDefInjection instead.")]
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