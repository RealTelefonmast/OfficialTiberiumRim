using TeleCore.Absorbed.Loader.Update;
using TeleCore.Logging;
using TeleCore.Utils;
using TeleCore.World.WorldInfo;
using UnityEngine;
using Verse;

namespace TeleCore.Static;

[StaticConstructorOnStartup]
public static class TFind
{
    private static readonly GameObject TeleRootHolder;
    private static readonly TeleRoot MainRoot;
    private static readonly TeleRoot_Ticking TickingRoot;

    static TFind()
    {
        TeleRootHolder = new GameObject("TeleCoreHolder");
        Object.DontDestroyOnLoad(TeleRootHolder);
        TeleRootHolder.AddComponent<TeleRoot>();

        MainRoot = TeleRootHolder.GetComponent<TeleRoot>();
        TickingRoot = TeleRootHolder.AddComponent<TeleRoot_Ticking>();

        TLog.Message("TFind Ready!", TColor.Green);
    }

    public static TeleRoot TeleRoot => MainRoot;
    public static TeleTickManager TickManager => TeleRoot.TickManager;
    public static DiscoveryTable Discoveries => StaticData.TeleWorldComp(Find.World.GetUniqueLoadID()).discoveries;

    public static GameComponent_CameraPanAndLock CameraPanNLock()
    {
        return Current.Game.GetComponent<GameComponent_CameraPanAndLock>();
    }

}