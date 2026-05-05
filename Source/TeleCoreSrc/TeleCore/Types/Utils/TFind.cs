using TeleCore.GameComponents;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

[StaticConstructorOnStartup]
public static class TFind
{
    private static readonly GameObject TeleRootHolder;
    private static readonly TeleRoot_Ticking TickingRoot;

    static TFind()
    {
        TeleRootHolder = new GameObject("TeleCoreHolder");
        Object.DontDestroyOnLoad(TeleRootHolder);
        TeleRootHolder.AddComponent<TeleRoot>();

        TeleRoot = TeleRootHolder.GetComponent<TeleRoot>();
        TickingRoot = TeleRootHolder.AddComponent<TeleRoot_Ticking>();

        TLog.Message("TFind Ready!", TColor.Green);
    }

    public static TeleRoot TeleRoot { get; }

    public static TeleTickManager TickManager => TeleRoot.TickManager;
    public static DiscoveryTable Discoveries => StaticData.TeleWorldComp(Find.World.GetUniqueLoadID()).discoveries;

    public static GameComponent_CameraPanAndLock CameraPanNLock()
    {
        return Current.Game.GetComponent<GameComponent_CameraPanAndLock>();
    }
}