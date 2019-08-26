using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TiberiumContent
    {
        static TiberiumContent() {}

        public static readonly Graphic_LinkedTNWOverlay TiberiumNetworkPipesOverlay = new Graphic_LinkedTNWOverlay(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas", ShaderDatabase.Transparent, Vector2.one, new ColorInt(155,255,0).ToColor));
        public static readonly Graphic_LinkedTNWOverlay TiberiumNetworkPipesGlow = new Graphic_LinkedTNWOverlay(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas", ShaderDatabase.MoteGlow, Vector2.one, Color.white));
        public static readonly Graphic_LinkedTNW TiberiumNetworkPipes = new Graphic_LinkedTNW(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeAtlas", ShaderDatabase.Transparent, Vector2.one, Color.white));

        public static readonly Texture2D DownArrow = ContentFinder<Texture2D>.Get("UI/Icons/DownArrow", true);

        public static readonly Texture2D BGPlanet = ContentFinder<Texture2D>.Get("UI/Menu/Background", true);
        //Faction Icons
        public static readonly Texture2D CommonIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Common", true);
        public static readonly Texture2D ForgottenIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Forgotten", true);
        public static readonly Texture2D GDIIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/GDI", true);
        public static readonly Texture2D NodIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Nod", true);
        public static readonly Texture2D ScrinIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Scrin", true);
        public static readonly Texture2D BlackMarketIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/BlackMarket", true);
        //Misc Icons
        public static readonly Texture2D TiberiumIcon = ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/TiberiumCategory", true);

        public static readonly Texture2D GeneralBG = ContentFinder<Texture2D>.Get("UI/Menu/Tapestry", true);

        //Harvester
        public static readonly Texture2D HarvesterRefinery = ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvester/NewRefinery", true);
        public static readonly Texture2D HarvesterReturn = ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvester/Return", true);
        public static readonly Texture2D HarvesterHarvest = ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvester/Harvest", true);
        public static readonly Texture2D HarvesterValue = ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvester/Value", true);
        public static readonly Texture2D HarvesterNearest = ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvester/Nearest", true);
        public static readonly Texture2D HarvesterMoss = ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvester/Moss", true);

        //Targeter Mats
        public static readonly Material IonCannonTargeter = MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);
        public static readonly Material NodNukeTargeter = MaterialPool.MatFrom("UI/Targeters/Target_Nuke", ShaderDatabase.Transparent);
        public static readonly Material ScrinLandingTargeter = MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);

        //Imported
        public static Shader AlphaShader;
        public static Material AlphaShaderMaterial;
    }
}
