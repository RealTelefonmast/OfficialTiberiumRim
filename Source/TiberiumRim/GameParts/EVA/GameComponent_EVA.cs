using System;
using System.Collections.Generic;
using RimWorld.Planet;
using TeleCore.ActionCompositions;
using TR.Util;
using Verse;
using Verse.Sound;

namespace TR.GameParts.EVA;

[StaticConstructorOnStartup]
public class GameComponent_EVA : GameComponent
{
    private static readonly Dictionary<EVAType, Dictionary<EVASignal, EVAMessageSoundDef>> messagyBySignal = new();
    private readonly Dictionary<EVASignal, int> LastPlayed = new();

    public GlobalTargetInfo EVATarget;
    public List<LocalTargetInfo> KnownTargets = new();

    private EVASettingsDef settings;
    private int tickSinceStart;
    private bool _evaActive;

    static GameComponent_EVA()
    {
        messagyBySignal.Add(EVAType.Common, new Dictionary<EVASignal, EVAMessageSoundDef>());
        //messagyBySignal.Add(EVAType.Nod, new Dictionary<EVASignal, EVAMessageDef>());
        //messagyBySignal.Add(EVAType.GDI, new Dictionary<EVASignal, EVAMessageDef>());
        //messagyBySignal.Add(EVAType.Scrin, new Dictionary<EVASignal, EVAMessageDef>());
    }

    public GameComponent_EVA(Game game)
    {
    }

    public Map Map => Find.CurrentMap;

    public World World => Find.World;

    public EVAType SelectedEVA { get; set; } = EVAType.None;

    public string EVAPrefix
    {
        get
        {
            switch (SelectedEVA)
            {
                case EVAType.None:
                    break;
                case EVAType.Common:
                    return "Ceva_";
                case EVAType.Nod:
                    return "Neva_";
                case EVAType.GDI:
                    return "Geva_";
                case EVAType.Scrin:
                    return "Aeva_";
            }

            return string.Empty;
        }
    }

    public bool EVAActive
    {
        get => _evaActive;
        set => _evaActive = value;
    }

    public bool CanPlay => SelectedEVA != EVAType.None && EVAActive;

    public static GameComponent_EVA EVAComp()
    {
        return Current.Game.GetComponent<GameComponent_EVA>();
    }

    public static void RegisterMessageDef(EVAMessageSoundDef messageDef)
    {
        //TRLog.Debug($"Registering EVAMessage [{messageDef.EVAType}][{messageDef.EVASignal}]({messageDef})");
        messagyBySignal[messageDef.EVAType].Add(messageDef.EVASignal, messageDef);
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        settings = DefDatabase<EVASettingsDef>.GetNamed("EVASettings");
        foreach (EVASignal signal in Enum.GetValues(typeof(EVASignal))) LastPlayed.Add(signal, 0);
    }

    public bool CanPlaySignal(EVASignal signal)
    {
        //TODO DEBUG
        return true;
        return tickSinceStart - LastPlayed[signal] >= settings.TimeFor(signal);
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
    }

    public override void GameComponentUpdate()
    {
        base.GameComponentUpdate();
        tickSinceStart++;
    }

    public void PlayCountDown(int seconds)
    {
        if (seconds > 10)
            seconds = 10;
        ActionComposition composition = new ActionComposition("EVACountDown");
        for (var i = 1; i <= seconds; i++)
            composition.AddPart(SoundDef.Named($"{EVAPrefix}Count{i}"), SoundInfo.OnCamera(), i);
        composition.Init();
    }

    public void RegisterTarget(LocalTargetInfo target)
    {
        KnownTargets.Add(target);
    }

    private void UpdateTargets()
    {
        KnownTargets.RemoveAll(t => !t.IsValid);
    }

    public void ReceiveSignal(EVASignal signal, LocalTargetInfo target)
    {
        if (!CanPlay) return;
        TRLog.Debug($"Received Signal {signal} at {target} with: CanPlay: {CanPlay} SelectedEVA: {SelectedEVA}");
        RegisterTarget(target);
        if (!messagyBySignal[SelectedEVA].TryGetValue(signal, out var evaMsg)) return;
        if (CanPlaySignal(signal))
        {
            evaMsg.PlayMessage(Map);
            LastPlayed[signal] = tickSinceStart;
            UpdateTargets();
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _evaActive, "isEVAActive");
    }
}

public enum EVASignal : int
{
    //EVA
    SystemsOnline,
    SystemsOffline,

    SilosNeeded,
    PowerOn,
    PowerOff,
    LowPower,
    PowerRestored,
    InsufficientFunds,

    //Base
    Building,
    OnHold,
    Cancelled,
    CantDeploy,
    ConComplete,
    BaseUnderAttack,
    BuildingLost,
    Repairing,

    //Units
    Training,
    HarvUndAttack,
    HarvesterLost,
    UnitUnderAttack,
    UnitLost,

    SelectLocation,
    SelectDestination,
    SelectDropzone,
    SelectTarget,
    SelectUnit,
    SelectWormhole,

    NewObjective,
    ObjectiveComplete,
    NewMission,

    //Warnings
    TiberiumExposure,
    TiberiumDepleted,

    //Toxic Exposure
    WarnSevereToxic,
    WarnHighToxic,
    WarnMildToxic,

    //Superweapons
    LiquidTibBombLaunched,
    NuclearMissLaunched,
    RiftGeneratorActivated,

    IonCannonReady,
    IonCannonActivated,
    CountD10,
    CountD09,
    CountD08,
    CountD07,
    CountD06,
    CountD05,
    CountD04,
    CountD03,
    CountD02,
    CountD01,
}
