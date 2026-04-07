using TeleCore.Network;
using Verse;

namespace TeleCore.Static;

internal static class Translations
{
    //Animation Tool
    internal static class AnimationStrings
    {
        public static readonly string NewAnimation = "TELE.AnimationTool.NewAnimation".Translate();
        public static readonly string DefName = "TELE.AnimationTool.NewAnimation.DefName".Translate();
        public static readonly string Init = "TELE.AnimationTool.NewAnimation.Init".Translate();
        public static readonly string MissingParts = "TELE.AnimationTool.Canvas.MissingParts".Translate();
        public static readonly string Settings = "TELE.AnimationTool.Settings".Translate();
        public static readonly string SettingsAnimations = "TELE.AnimationTool.Settings.Animations".Translate();
        public static readonly string SettingsSets = "TELE.AnimationTool.Settings.AnimationSets".Translate();
        public static readonly string SettingsAddPart = "TELE.AnimationTool.Settings.AddPart".Translate();
        public static readonly string SettingsAddSide = "TELE.AnimationTool.Settings.AddSide".Translate();

        public static readonly string SaveAnimFile = "TELE.AnimationTool.IO.SaveFile".Translate();
        public static readonly string LoadAnimFile = "TELE.AnimationTool.IO.LoadFile".Translate();
        public static readonly string DeleteAnimFile = "TELE.AnimationTool.IO.DeleteFile".Translate();
        public static readonly string SaveAnimDef = "TELE.AnimationTool.IO.SaveDefFile".Translate();
    }

    //
    internal static class NetworkStrings
    {
        public static readonly string PortableContainer = "TELE.PortableContainer.Title".Translate();
        public static readonly string NetworkBillsITab = "TELE.ITab.NetworkBills".Translate();
    }

    internal static class Hediffs
    {
        public static readonly string ExplodedHediffRuptured = "TELE.Hediffs.Comps.ExplodedHediffRuptured".Translate();
    }

    internal static class Discovery
    {
        public static readonly string DiscoveryNew = "TELE.Discovery.New".Translate();
        public static readonly string DiscoveryTab = "TELE.Discovery.Tab".Translate();

        public static TaggedString DiscoveryDesc(string discoveryDesc)
        {
            return "TELE.Discovery.Desc".Translate(discoveryDesc);
        }
    }

    public class Messages
    {
        public static string NoPortableContainer(INetworkPart forComp)
        {
            return "TELE.Messages.NoPortableContainer".Translate(forComp.Config.networkDef);
        }
    }

    public class PlaceWorker
    {
        public static string OnlyOutside => "TELE.PlaceWorker.OnlyOutside".Translate();
    }

    public static class Buildings
    {
        public static string TurretHubAnticipated(int count) => "TELE.Buildings.TurretHub.Anticipated".Translate(count);
    }
}