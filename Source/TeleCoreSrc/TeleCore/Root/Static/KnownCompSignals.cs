namespace TeleCore.Static;

public static class KnownCompSignals
{
    //Door
    public const string DoorOpened = "DoorOpened";
    public const string DoorClosed = "DoorClosed";
    
    //Flicking
    public const string FlickedOn = "FlickedOn";
    public const string FlickedOff = "FlickedOff";

    //Power
    public const string PowerTurnedOn = "PowerTurnedOn";
    public const string PowerTurnedOff = "PowerTurnedOff";

    //Refuelable
    public const string Refueled = "Refueled";
    public const string RanOutOfFuel = "RanOutOfFuel";

    //
    public const string CrateContentsChanged = "CrateContentsChanged";
    public const string Breakdown = "Breakdown";
    public const string Hackend = "Hackend";
    public const string AutoPoweredWantsOff = "AutoPoweredWantsOff";
    public const string RitualTargetChanged = "RitualTargetChanged";
    public const string ScheduledOn = "ScheduledOn";
    public const string ScheduledOff = "ScheduledOff";
    public const string RuinedByTemperature = "RuinedByTemperature";
    public const string MechClusterDefeated = "MechClusterDefeated";
}