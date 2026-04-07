using System.Linq;
using RimWorld;
using TR.Components;
using TR.Defs;
using TR.TiberiumEnvironment;
using UnityEngine;
using Verse;
using WorldComponent_TR = TR.World.WorldComponent_TR;

namespace TR.TiberiumObjects;

public class TiberiumPlant : Plant
{
    private static readonly Color32[] colors = new Color32[4];

    private Graphic graphicInt2;

    public TiberiumGarden parentGarden;
    public new TRThingDef def => (TRThingDef)base.def;

    public WorldComponent_TR TiberiumRimComp => Find.World.GetComponent<WorldComponent_TR>();
    public MapComponent_Tiberium TiberiumMapComp => Map.GetComponent<MapComponent_Tiberium>();
    public override bool BlightableNow => false;
    public override bool IngestibleNow => false;

    public override float CurrentDyingDamagePerTick => base.CurrentDyingDamagePerTick;

    public Graphic OverlayGraphic
    {
        get
        {
            if (graphicInt2 == null)
            {
                if (def.graphicData2 == null) return null;
                graphicInt2 = def.graphicData2.GraphicColoredFor(this);
                return graphicInt2;
                if (Graphic is Graphic_Random random)
                {
                    var path = def.graphicData2.texPath;
                    var graphic = random.SubGraphicFor(this);
                    var suffix = graphic.path.Split('/').Last();
                    Log.Message("Graphic: " + graphic.path + " | suffix: " + suffix);
                    path += "/" + suffix;
                    graphicInt2 = GraphicDatabase.Get(typeof(Graphic_Single), path, def.graphicData2.shaderType.Shader,
                        def.graphicData2.drawSize, def.graphicData2.color, def.graphicData2.colorTwo);
                }
                else
                {
                    graphicInt2 = def.graphicData2.GraphicColoredFor(this);
                }
            }

            return graphicInt2;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TiberiumMapComp.RegisterTiberiumPlant(this);
    }

    public override void PostMake()
    {
        base.PostMake();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        TiberiumMapComp.DeregisterTiberiumPlant(this);
        base.DeSpawn(mode);
    }

    public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostApplyDamage(dinfo, totalDamageDealt);
    }

    public override void Draw()
    {
        base.Draw();
    }

    public override void Print(SectionLayer layer)
    {
        base.Print(layer);
    }
}