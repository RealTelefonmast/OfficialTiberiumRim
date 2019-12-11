using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TRThingDef : FXThingDef
    {
        public FactionDesignationDef factionDesignation = FactionDesignationDefOf.Tiberium;
        public TRThingCategoryDef TRCategory = TRCategoryDefOf.Misc;
        public GraphicData extraGraphicData;
        public TurretHolderProps turret;
        public BeamHubProperties beamHub; 
        public ProjectileProperties_Extended projectileExtended;
        public SuperWeaponProperties superWeapon;
        public TerrainDef makesTerrain;
        public bool needsBlueprint = false;
        public bool hidden = false;
        public bool devObject = false;
        public bool destroyTiberium = false;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
        }
    }

    public class FactionDesignationDef : Def
    {
        public List<TRThingCategoryDef> subCategories = new List<TRThingCategoryDef>();
        public string packPath = "";
    }

    public class DesignationTexturePack
    {
        public Texture2D BackGround;
        public Texture2D Tab;
        public Texture2D TabSelected;
        public Texture2D Designator;
        public Texture2D DesignatorSelected;

        public DesignationTexturePack(FactionDesignationDef def)
        {
            BackGround = ContentFinder<Texture2D>.Get(def.packPath + "/" + "BuildMenu");
            Tab = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Tab");
            TabSelected = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Tab_Selected");
            Designator = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Des");
            DesignatorSelected = ContentFinder<Texture2D>.Get((def.packPath + "/" + "Des_Selected"));
        }
    }

    public class TRThingCategoryDef : Def
    {
    }
}
