using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace TiberiumRim;

public enum EVA
{
    None,
    Nod,
    GDI,
    Scrin
}

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

public class GameComponent_EVA : GameComponent
{
    public GlobalTargetInfo EVATarget;

    public List<LocalTargetInfo> KnownTargets = new();
    public Dictionary<EVASignal, int> LastPlayed = new();

    public EVASettings settings;
    private int tickSinceStart;

    public GameComponent_EVA(Game game)
    {
    }

    public Map Map => Find.CurrentMap;

    public World World => Find.World;

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

    public bool CanPlay => TiberiumRimSettings.settings.EVASystem;

    public static GameComponent_EVA EVAComp()
    {
        return Current.Game.GetComponent<GameComponent_EVA>();
    }

    public override void FinalizeInit()
    {
        base.FinalizeInit();
        settings = DefDatabase<EVASettings>.GetNamed("EVASettings");
        foreach (EVASignal signal in Enum.GetValues(typeof(EVASignal))) LastPlayed.Add(signal, 0);
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
    }

    public override void GameComponentUpdate()
    {
        base.GameComponentUpdate();
        tickSinceStart++;
        //Log.Message("TicksGame: " + Find.TickManager.TicksGame + " | " + GenTicks.TicksGame+ "TicksAbs: " + Find.TickManager.TicksAbs + " | " + GenTicks.TicksAbs);
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

    public void RegisterTarget(Thing thing)
    {
        KnownTargets.Add(new LocalTargetInfo(thing));
    }

    private void UpdateTargets()
    {
        KnownTargets.RemoveAll(t => !t.IsValid);
    }

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
}

public enum EVASignal
{
    SILOSNEEDED,
    PowerOn,
    PowerOff,
    LowPower,
    PowerRestored,
    InsufficientFunds,
    BaseUnderAttack,
    BuildingLost,
    UnitUnderAttack,
    UnitLost,
    CantDeploy,
    Canceled,
    OnHold,
    Building,

    SelectLocation,
    SelectDestination,
    SelectDropzone,
    SelectTarget,
    SelectUnit,
    SelectWormhole,

    NewObjective,
    ObjectiveComplete,
    NewMission,

    TiberiumExposure,

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