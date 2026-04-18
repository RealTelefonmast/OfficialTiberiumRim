using Verse;

namespace TeleCore.Unsorted;

public class VerbComponent
{
    private TeleVerbAttacher _parent;
    
    public Verb Verb => _parent.Verb;


    public void Notify_WarmupComplete()
    {
        throw new System.NotImplementedException();
    }

    public void Notify_ShotCast()
    {
        throw new System.NotImplementedException();
    }

    public void Notify_Reset()
    {
        throw new System.NotImplementedException();
    }

    public void Notify_ProjectileLaunched(Projectile obj)
    {
        throw new System.NotImplementedException();
    }
}