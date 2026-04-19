using Verse;

namespace TR.Info;

public class MapInformation : IExposable
{
    private bool initialized;
    protected Map map;

    public MapInformation(Map map)
    {
        this.map = map;
    }

    public bool HasBeenInitialized => initialized;

    public Map Map => map;

    public virtual void ExposeData()
    {
        Scribe_Values.Look(ref initialized, "mapInfoInit");
    }

    public virtual void InfoInit(bool initAfterReload = false)
    {
        initialized = true;
    }

    public virtual void Tick()
    {
    }

    public virtual void UpdateOnGUI()
    {
    }

    public virtual void Draw()
    {
    }
}