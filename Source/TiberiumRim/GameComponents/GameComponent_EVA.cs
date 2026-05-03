using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace TR;

// B — newer faction enum (replaces EVAType.Common)
public enum EVA
{
    None,
    Nod,
    GDI,
    Scrin
}

// B — Def-based settings replacing EVASettingsDef
public class EVASettings : Def
{
    public List<EVATime> times;

    public int TimeFor(EVASignal signal)
    {
        return times.Find(t => t.signal == signal).ticks;
    }
}

public class EVATime
{
    public EVASignal signal;
    public int ticks = 500;
}

[StaticConstructorOnStartup]
public class GameComponent_EVA : GameComponent
{
    // A — def-based sound lookup (EVAType still referenced from elsewhere in codebase)
    private static readonly Dictionary<EVAType, Dictionary<EVASignal, EVAMessageSoundDef>> messagyBySignal = new();

    public GlobalTargetInfo EVATarget;
    public List<LocalTargetInfo> KnownTargets = new();
    // B — public (not readonly) so it can be serialized / inspected
    public Dictionary<EVASignal, int> LastPlayed = new();

    // A — EVAActive persistence
    private bool _evaActive;

    // B — canonical settings type
    public EVASettings settings;
    private int tickSinceStart;

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

    // B — canonical EVA enum, default Scrin
    public EVA SelectedEVA { get; set; } = EVA.Scrin;

    public string EVAPrefix
    {
        get
        {
            var str = "";
            switch (SelectedEVA)
            {
                case EVA.None:
                    break;
                case EVA.Nod:
                    str = "Neva_";
                    break;
                case EVA.GDI:
                    str = "Geva_";
                    break;
                case EVA.Scrin:
                    str = "Aeva_";
                    break;
            }

            return str;
        }
    }

    // A — activation flag with persistence
    public bool EVAActive
    {
        get => _evaActive;
        set => _evaActive = value;
    }

    // B — canonical: driven by mod settings
    public bool CanPlay => TiberiumRimSettings.settings.EVASystem;

    public static GameComponent_EVA EVAComp()
    {
        return Current.Game.GetComponent<GameComponent_EVA>();
    }

    // A — registers a sound def into the legacy dict-based lookup
    public static void RegisterMessageDef(EVAMessageSoundDef messageDef)
    {
        //TRLog.Debug($"Registering EVAMessage [{messageDef.EVAType}][{messageDef.EVASignal}]({messageDef})");
        messagyBySignal[messageDef.EVAType].Add(messageDef.EVASignal, messageDef);
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        settings = DefDatabase<EVASettings>.GetNamed("EVASettings");
        foreach (EVASignal signal in Enum.GetValues(typeof(EVASignal))) LastPlayed.Add(signal, 0);
    }

    // A — cooldown guard; real logic unreachable (TODO DEBUG)
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
        //Log.Message("TicksGame: " + Find.TickManager.TicksGame + " | " + GenTicks.TicksGame + "TicksAbs: " + Find.TickManager.TicksAbs + " | " + GenTicks.TicksAbs);
    }

    public void PlayCountDown(int seconds)
    {
        if (seconds > 10)
            seconds = 10;
        ActionComposition composition = new ActionComposition("EVACountDown");
        for (var i = 1; i <= seconds; i++)
            composition.AddPart(SoundDef.Named(EVAPrefix + "Count" + i), SoundInfo.OnCamera(), i);
        composition.Init();
    }

    // A — direct LocalTargetInfo
    public void RegisterTarget(LocalTargetInfo target)
    {
        KnownTargets.Add(target);
    }

    // B — wraps Thing into LocalTargetInfo
    public void RegisterTarget(Thing thing)
    {
        KnownTargets.Add(new LocalTargetInfo(thing));
    }

    private void UpdateTargets()
    {
        KnownTargets.RemoveAll(t => !t.IsValid);
    }

    // A — legacy dict-based signal dispatch with target registration
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

    // B — canonical: inline switch building SoundDef, inline cooldown check
    public void ReceiveSignal(EVASignal signal)
    {
        Log.Message("Received EVA signal: " + signal);
        if (!CanPlay) return;

        SoundDef soundToPlay = null;
        switch (signal)
        {
            case EVASignal.SILOSNEEDED:
                soundToPlay = SoundDef.Named(EVAPrefix + "SilosNeeded");
                break;
            case EVASignal.PowerOn:
                soundToPlay = SoundDef.Named(EVAPrefix + "BuildingOn");
                break;
            case EVASignal.PowerOff:
                soundToPlay = SoundDef.Named(EVAPrefix + "BuildingOff");
                break;
            case EVASignal.BuildingLost:
                soundToPlay = SoundDef.Named(EVAPrefix + "BuildingLost");
                break;
            case EVASignal.LowPower:
                soundToPlay = SoundDef.Named(EVAPrefix + "LowPower");
                break;
            case EVASignal.InsufficientFunds:
                soundToPlay = SoundDef.Named(EVAPrefix + "InsufficFunds");
                break;
            case EVASignal.BaseUnderAttack:
                soundToPlay = SoundDef.Named(EVAPrefix + "BaseUndAttack");
                break;
            case EVASignal.UnitUnderAttack:
                soundToPlay = SoundDef.Named(EVAPrefix + "UnitUndAttack");
                break;
            case EVASignal.PowerRestored:
                soundToPlay = SoundDef.Named(EVAPrefix + "PowerRestored");
                break;
            case EVASignal.UnitLost:
                soundToPlay = SoundDef.Named(EVAPrefix + "UnitLost");
                break;
            case EVASignal.CantDeploy:
                soundToPlay = SoundDef.Named(EVAPrefix + "CantDeploHere");
                break;
            case EVASignal.OnHold:
                soundToPlay = SoundDef.Named(EVAPrefix + "OnHold");
                break;
            case EVASignal.Canceled:
                soundToPlay = SoundDef.Named(EVAPrefix + "Canceled");
                break;
            case EVASignal.Building:
                soundToPlay = SoundDef.Named(EVAPrefix + "Building");
                break;
            case EVASignal.SelectLocation:
                soundToPlay = SoundDef.Named(EVAPrefix + "SelectLocation");
                break;
            case EVASignal.SelectDestination:
                soundToPlay = SoundDef.Named(EVAPrefix + "SelectDestination");
                break;
            case EVASignal.SelectDropzone:
                soundToPlay = SoundDef.Named(EVAPrefix + "SelectDropzone");
                break;
            case EVASignal.SelectTarget:
                soundToPlay = SoundDef.Named(EVAPrefix + "SelectTarget");
                break;
            case EVASignal.SelectUnit:
                soundToPlay = SoundDef.Named(EVAPrefix + "SelectUnit");
                break;
            case EVASignal.SelectWormhole:
                soundToPlay = SoundDef.Named(EVAPrefix + "SelectWormhole");
                break;
            case EVASignal.IonCannonReady:
                soundToPlay = SoundDef.Named(EVAPrefix + "IonCannonReady");
                break;
            case EVASignal.IonCannonActivated:
                soundToPlay = SoundDef.Named(EVAPrefix + "IonCannonActivated");
                break;
            case EVASignal.NewObjective:
                break;
            case EVASignal.ObjectiveComplete:
                break;
            case EVASignal.NewMission:
                break;
            case EVASignal.TiberiumExposure:
                soundToPlay = SoundDef.Named(EVAPrefix + "TiberExposDet");
                break;
            case EVASignal.CountD10:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count10");
                break;
            case EVASignal.CountD09:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count09");
                break;
            case EVASignal.CountD08:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count08");
                break;
            case EVASignal.CountD07:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count07");
                break;
            case EVASignal.CountD06:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count06");
                break;
            case EVASignal.CountD05:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count05");
                break;
            case EVASignal.CountD04:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count04");
                break;
            case EVASignal.CountD03:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count03");
                break;
            case EVASignal.CountD02:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count02");
                break;
            case EVASignal.CountD01:
                soundToPlay = SoundDef.Named(EVAPrefix + "Count01");
                break;
        }

        if (tickSinceStart - LastPlayed[signal] >= settings.TimeFor(signal))
        {
            soundToPlay?.PlayOneShotOnCamera(Map);
            LastPlayed[signal] = tickSinceStart;
            UpdateTargets();
        }
        //Log.Message("Can't be played - wait " + (settings.TimeFor(signal) - (tickSinceStart - LastPlayed[signal])) + " ticks");
    }

    // A — persists EVAActive across saves
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref _evaActive, "isEVAActive");
    }
}

public enum EVASignal
{
    //EVA
    SystemsOnline,      // A
    SystemsOffline,     // A

    SILOSNEEDED,        // B canonical
    SilosNeeded,        // A (alternate casing — may be referenced elsewhere)
    PowerOn,
    PowerOff,
    LowPower,
    PowerRestored,
    InsufficientFunds,

    //Base
    Building,
    OnHold,
    Canceled,           // B canonical
    Cancelled,          // A (alternate spelling — may be referenced elsewhere)
    CantDeploy,
    ConComplete,        // A
    BaseUnderAttack,
    BuildingLost,
    Repairing,          // A

    //Units
    Training,           // A
    HarvUndAttack,      // A
    HarvesterLost,      // A
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
    TiberiumDepleted,   // A

    //Toxic Exposure
    WarnSevereToxic,    // A
    WarnHighToxic,      // A
    WarnMildToxic,      // A

    //Superweapons
    LiquidTibBombLaunched,  // A
    NuclearMissLaunched,    // A
    RiftGeneratorActivated, // A

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
    CountD01
}
