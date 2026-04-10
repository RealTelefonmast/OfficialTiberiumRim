using TeleCore.GameData.Defs.Extensions;
using Verse;

namespace TeleCore.Verbs;

public class BeamProperties
{
    public float? armorPenetrationOverride;
    public int? damageBaseOverride = 100;

    //
    public DamageDef damageDef;
    public FloatRange visWidth = FloatRange.One;
    
    //
    public EffecterDef impactEffecter;
    public ExplosionProperties impactExplosion;
    public FilthSpawnerProperties impactFilth;

    public bool isStatic = false;

    [Unsaved] private VerbProperties_Extended parent;

    public bool spawnMotePerBeam = false;
    public float staggerTime = 95.TicksToSeconds();
    public float? stoppingPowerOverride;

    public ThingDef BeamMoteDef => parent.beamMoteDef;
    public FleckDef BeamGroundFleckDef => parent.beamGroundFleckDef;
    public float BeamWidth => parent.beamWidth;
    public float BeamMaxDeviation => parent.beamMaxDeviation;
    public FloatRange VisualWidthRange => visWidth;


    public void SetParent(VerbProperties_Extended verbprops)
    {
        parent = verbprops;
    }
}