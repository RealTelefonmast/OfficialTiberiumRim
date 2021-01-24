using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TiberiumContent
    {
        static TiberiumContent() { }

        public static readonly Graphic_LinkedTNWOverlay TiberiumNetworkPipesOverlay = new Graphic_LinkedTNWOverlay(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas", ShaderDatabase.Transparent, Vector2.one, new ColorInt(155, 255, 0).ToColor));
        public static readonly Graphic_LinkedTNWOverlay TiberiumNetworkPipesGlow = new Graphic_LinkedTNWOverlay(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas", ShaderDatabase.MoteGlow, Vector2.one, Color.white));
        public static readonly Graphic_LinkedTNW TiberiumNetworkPipes = new Graphic_LinkedTNW(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeAtlas", ShaderDatabase.Transparent, Vector2.one, Color.white));

        //Icons
        public static readonly Texture2D MissingConnection = ContentFinder<Texture2D>.Get("UI/Icons/TiberiumNetwork/ConnectionMissing", false);
        public static readonly Texture2D MarkedForDeath = ContentFinder<Texture2D>.Get("UI/Icons/Marked", false);
        public static readonly Texture2D Icon_EVA = ContentFinder<Texture2D>.Get("UI/Icons/EVA", false);

        //Turrets
        public static readonly Material TurretCable = MaterialPool.MatFrom("Buildings/Nod/Defense/Turrets/TurretCable");

        //UI - Menus
        public static readonly Texture2D BGPlanet = ContentFinder<Texture2D>.Get("UI/Menu/Background", true);
        public static readonly Texture2D ResearchBG = ContentFinder<Texture2D>.Get("UI/Menu/ResearchBG", true);
        public static readonly Texture2D MainMenu = ContentFinder<Texture2D>.Get("UI/Menu/MainMenu", true);
        public static readonly Texture2D MenuWindow = ContentFinder<Texture2D>.Get("UI/Menu/MenuWindow", true);
        public static readonly Texture2D Banner = ContentFinder<Texture2D>.Get("UI/Menu/Banner", true);
        public static readonly Texture2D LockedBanner = ContentFinder<Texture2D>.Get("UI/Menu/LockedBanner", true);

        public static readonly Texture2D Undiscovered = ContentFinder<Texture2D>.Get("UI/Menu/Undiscovered", true);
        public static readonly Texture2D Fact_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Fact_Undiscovered", true);
        public static readonly Texture2D Des_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Des_Undiscovered", true);
        public static readonly Texture2D Tab_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Tab_Undiscovered", true);
        public static readonly Texture2D InfoButton = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);
        public static readonly Texture2D SideBarArrow = ContentFinder<Texture2D>.Get("UI/Menu/Arrow", true);

        public static readonly Texture2D HighlightAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TutorHighlightAtlas", true);

        public static readonly Texture2D AttentionMarker = ContentFinder<Texture2D>.Get("UI/Icons/AttentionMarker", true);
        public static readonly Texture2D NewMarker = ContentFinder<Texture2D>.Get("UI/Icons/NewMarker", true);

        //public static readonly Texture2D HightlightInMenu = ContentFinder<Texture2D>.Get("UI/Icons/HighLight", true);
        public static readonly Texture2D OpenMenu = ContentFinder<Texture2D>.Get("UI/Icons/OpenMenu", true);
        public static readonly Texture2D Construct = ContentFinder<Texture2D>.Get("UI/Icons/Construct", true);
        public static readonly Texture2D SelectThing = ContentFinder<Texture2D>.Get("UI/Icons/SelectThing", true);
        //UI - Icons
        //--Controls
        //----Harvester
        public static readonly Texture2D HarvesterRefinery = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/NewRefinery", true);
        public static readonly Texture2D HarvesterReturn = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Return", true);
        public static readonly Texture2D HarvesterHarvest = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Harvest", true);
        public static readonly Texture2D HarvesterValue = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Value", true);
        public static readonly Texture2D HarvesterNearest = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Nearest", true);
        public static readonly Texture2D HarvesterMoss = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Moss", true);

        //----SuperWeapon
        public static readonly Texture2D NodNukeIcon = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Launch_Nuke", true);
        public static readonly Texture2D IonCannonIcon = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Launch_IonCannon", true);
        public static readonly Texture2D FireStorm_On = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Firestorm_On", true);
        public static readonly Texture2D FireStorm_Off = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Firestorm_Off", true);

        //Harvester Bar
        public static readonly Material Harvester_EmptyBar = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);
        public static readonly Material Harvester_FilledBar = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 1f, 1f, 1f), ShaderDatabase.MetaOverlay);


        //----Tib Container
        public static readonly Texture2D ContainMode_Sludge = ContentFinder<Texture2D>.Get("UI/Icons/Controls/TibContainer/ContainMode_Sludge", true);
        public static readonly Texture2D ContainMode_TripleSwitch = ContentFinder<Texture2D>.Get("UI/Icons/Controls/TibContainer/ContainMode_Storage", true);

        //--Faction Icons
        public static readonly Texture2D CommonIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Common", true);
        public static readonly Texture2D ForgottenIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Forgotten", true);
        public static readonly Texture2D GDIIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/GDI", true);
        public static readonly Texture2D NodIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Nod", true);
        public static readonly Texture2D ScrinIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Scrin", true);
        public static readonly Texture2D BlackMarketIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/BlackMarket", true);

        //--Tiberium Icons
        public static readonly Texture2D GreenTiberium = ContentFinder<Texture2D>.Get("Tiberium/Green/Tiberium_Green3");
        public static readonly Texture2D BlueTiberium = ContentFinder<Texture2D>.Get("Tiberium/Blue/Tiberium_blue3");
        public static readonly Texture2D RedTiberium = ContentFinder<Texture2D>.Get("Tiberium/Red/Tiberium_Red2");

        //--World
        public static readonly Material Infested_1 = MaterialPool.MatFrom("World/Tile/Tib_1", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material Infested_2 = MaterialPool.MatFrom("World/Tile/Tib_2", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material Infested_3 = MaterialPool.MatFrom("World/Tile/Tib_3", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material Infested_4 = MaterialPool.MatFrom("World/Tile/Tib_4", ShaderDatabase.WorldOverlayTransparentLit, 3505);

        public static readonly Material TibTile_1 = MaterialPool.MatFrom("World/Old/Tib_1", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_2 = MaterialPool.MatFrom("World/Old/Tib_2", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_3 = MaterialPool.MatFrom("World/Old/Tib_3", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_4 = MaterialPool.MatFrom("World/Old/Tib_4", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_Glacier = MaterialPool.MatFrom("World/Old/Tib_4", ShaderDatabase.WorldOverlayTransparentLit, 3505);

        //--Research
        public static readonly Texture2D Research_Active = ContentFinder<Texture2D>.Get("UI/Icons/Research/Active");
        public static readonly Texture2D Research_Available = ContentFinder<Texture2D>.Get("UI/Icons/Research/Available", true);

        //--Hediffs
        public static readonly Texture2D Hediff_Crystallizing = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Crystallizing", true);
        public static readonly Texture2D Hediff_Mutation = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Mutation", true);
        public static readonly Texture2D Hediff_Radiation = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Radiation", true);
        public static readonly Texture2D Hediff_Immunity = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Immunity", true);

        //ThingCategories
        public static readonly Texture2D TiberiumIcon = ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/TiberiumCategory", true);

        //Tiberium Network
        public static readonly Texture2D Network_MissingConnection = ContentFinder<Texture2D>.Get("UI/Icons/TiberiumNetwork/ConnectionMissing", true);

        //Targeter Mats
        public static readonly Material IonCannonTargeter = MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);
        public static readonly Material NodNukeTargeter = MaterialPool.MatFrom("UI/Targeters/Target_Nuke", ShaderDatabase.Transparent);
        public static readonly Material ScrinLandingTargeter = MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);

        public static readonly Material IonLightningMat = MaterialPool.MatFrom("Motes/LightningBoltIon", ShaderDatabase.MoteGlow);

        public static readonly Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

        //Imported
        public static Shader AlphaShader;
        public static Material AlphaShaderMaterial;
    }
}
