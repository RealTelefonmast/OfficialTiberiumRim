using Verse;

namespace TR.Components;

public abstract class MapComponent_Info : MapComponent
{
    public MapComponent_Info(TeleCore.Map map) : base(map)
    {
    }

    public sealed override void FinalizeInit()
    {
        base.FinalizeInit();
        Initialize();

        //Provide Thread-Safe Initializer
        LongEventHandler.QueueLongEvent(ThreadSafeInit, string.Empty, false, null, false);
    }

    protected virtual void Initialize()
    {
    }


    /// <summary>
    ///     Thread safe initializer for data on the main game thread
    /// </summary>
    public virtual void ThreadSafeInit()
    {
    }
}