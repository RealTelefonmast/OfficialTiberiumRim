using UnityEngine;
using Verse;

namespace TiberiumRim;

[StaticConstructorOnStartup]
public class TiberiumContent
{
    public static readonly Graphic_LinkedTNWOverlay TiberiumNetworkPipesOverlay =
        new(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas",
            ShaderDatabase.Transparent, Vector2.one, new ColorInt(155, 255, 0).ToColor));

    public static readonly Graphic_LinkedTNWOverlay TiberiumNetworkPipesGlow =
        new(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas",
            ShaderDatabase.MoteGlow, Vector2.one, Color.white));

    public static readonly Graphic_LinkedTNW TiberiumNetworkPipes =
        new(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeAtlas", ShaderDatabase.Transparent,
            Vector2.one, Color.white));

    //Icons
    public static readonly Texture2D MissingConnection =
        ContentFinder<Texture2D>.Get("UI/Icons/TiberiumNetwork/ConnectionMissing", false);

    public static readonly Texture2D MarkedForDeath = ContentFinder<Texture2D>.Get("UI/Icons/Marked", false);
    public static readonly Texture2D Icon_EVA = ContentFinder<Texture2D>.Get("UI/Icons/EVA", false);

    //Turrets
    public static readonly Material TurretCable = MaterialPool.MatFrom("Buildings/Nod/Defense/Turrets/TurretCable");

    //UI - Menus
    public static readonly Texture2D BGPlanet = ContentFinder<Texture2D>.Get("UI/Menu/Background");
    public static readonly Texture2D ResearchBG = ContentFinder<Texture2D>.Get("UI/Menu/ResearchBG");
    public static readonly Texture2D MainMenu = ContentFinder<Texture2D>.Get("UI/Menu/MainMenu");
    public static readonly Texture2D MenuWindow = ContentFinder<Texture2D>.Get("UI/Menu/MenuWindow");
    public static readonly Texture2D Banner = ContentFinder<Texture2D>.Get("UI/Menu/Banner");

    public static readonly Texture2D Undiscovered = ContentFinder<Texture2D>.Get("UI/Menu/Undiscovered");
    public static readonly Texture2D Fact_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Fact_Undiscovered");
    public static readonly Texture2D Des_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Des_Undiscovered");
    public static readonly Texture2D Tab_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Tab_Undiscovered");
    public static readonly Texture2D InfoButton = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton");

    //UI - Icons
    //--Controls
    //----Harvester
    public static readonly Texture2D HarvesterRefinery =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/NewRefinery");

    public static readonly Texture2D HarvesterReturn =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Return");

    public static readonly Texture2D HarvesterHarvest =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Harvest");

    public static readonly Texture2D HarvesterValue = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Value");

    public static readonly Texture2D HarvesterNearest =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Nearest");

    public static readonly Texture2D HarvesterMoss = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Moss");

    //----SuperWeapon
    public static readonly Texture2D NodNukeIcon =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Launch_Nuke");

    public static readonly Texture2D IonCannonIcon =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Launch_IonCannon");

    public static readonly Texture2D FireStorm_On =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Firestorm_On");

    public static readonly Texture2D FireStorm_Off =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Firestorm_Off");

    //----Tib Container
    public static readonly Texture2D ContainMode_Sludge =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/TibContainer/ContainMode_Sludge");

    public static readonly Texture2D ContainMode_TripleSwitch =
        ContentFinder<Texture2D>.Get("UI/Icons/Controls/TibContainer/ContainMode_Storage");

    //--Faction Icons
    public static readonly Texture2D CommonIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Common");
    public static readonly Texture2D ForgottenIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Forgotten");
    public static readonly Texture2D GDIIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/GDI");
    public static readonly Texture2D NodIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Nod");
    public static readonly Texture2D ScrinIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Scrin");
    public static readonly Texture2D BlackMarketIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/BlackMarket");

    //--Research
    public static readonly Texture2D Research_Active = ContentFinder<Texture2D>.Get("UI/Icons/Research/Active");
    public static readonly Texture2D Research_Available = ContentFinder<Texture2D>.Get("UI/Icons/Research/Available");

    //--Hediffs
    public static readonly Texture2D Hediff_Crystallizing =
        ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Crystallizing");

    public static readonly Texture2D Hediff_Mutation = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Mutation");
    public static readonly Texture2D Hediff_TibImmune = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/TiberiumImmune");

    //ThingCategories
    public static readonly Texture2D TiberiumIcon =
        ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/TiberiumCategory");

    //Tiberium Network
    public static readonly Texture2D Network_MissingConnection =
        ContentFinder<Texture2D>.Get("UI/Icons/TiberiumNetwork/ConnectionMissing");

    //Targeter Mats
    public static readonly Material IonCannonTargeter =
        MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);

    public static readonly Material NodNukeTargeter =
        MaterialPool.MatFrom("UI/Targeters/Target_Nuke", ShaderDatabase.Transparent);

    public static readonly Material ScrinLandingTargeter =
        MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);

    public static readonly Material IonLightningMat =
        MaterialPool.MatFrom("Motes/LightningBoltIon", ShaderDatabase.MoteGlow);

    public static readonly Material ForcedTargetLineMat =
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

    //Imported
    public static Shader AlphaShader;
    public static Material AlphaShaderMaterial;

    static TiberiumContent()
    {
    }
}