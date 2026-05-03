using System;
using Verse;

namespace TeleCore.Unsorted;

public class VerbComponent
{
    private TeleVerbAttacher _parent;

    public Verb Verb => _parent.Verb;


    public void Notify_WarmupComplete()
    {
        throw new NotImplementedException();
    }

    public void Notify_ShotCast()
    {
        throw new NotImplementedException();
    }

    public void Notify_Reset()
    {
        throw new NotImplementedException();
    }

    public void Notify_ProjectileLaunched(Projectile obj)
    {
        throw new NotImplementedException();
    }
}