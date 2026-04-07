using RimWorld;
using TR.Rendering.TextureContent;
using TR.Util;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TR.Weather;

public class WeatherEvent_IonStrike : WeatherEvent
{
    private const int FlashFadeInTicks = 3;
    private const int MinFlashDuration = 15;
    private const int MaxFlashDuration = 60;

    private const float FlashShadowDistance = 5f;

    //RGB (250,250,80)
    //private static readonly SkyColorSet LightningFlashColors = new SkyColorSet(new Color(0.980392156f‬, 0.980392156f‬, 0.012254901f), new Color(0.784313738f, 0.8235294f, 0.847058833f), new Color(0.9f, 0.95f, 1f), 1.15f);
    private static readonly SkyColorSet LightningFlashColors = new(new ColorInt(250, 250, 80).ToColor,
        new Color(0.784313738f, 0.8235294f, 0.847058833f),
        new Color(0.9f, 0.95f, 1f), 0.8f);

    private static readonly SkyTarget Target = new(1, LightningFlashColors, 1, 1);

    private readonly int duration;
    private readonly Vector2 shadowVector;
    private int ageTicks;

    private Mesh boltMesh;
    private IntVec3 strikeLoc = IntVec3.Invalid;

    public WeatherEvent_IonStrike(Map map) : base(map)
    {
        duration = TRUtils.Range(15, 60);
        shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
    }

    public override bool Expired => ageTicks > duration;
    public override float SkyTargetLerpFactor => LightningBrightness;
    public override SkyTarget SkyTarget => Target;
    public override Vector2? OverrideShadowVector => shadowVector;

    private float LightningBrightness
    {
        get
        {
            if (ageTicks <= 3) return ageTicks / 3f;
            return 1f - ageTicks / (float)duration;
        }
    }

    private bool DoStrike => TRUtils.Chance(0.4f);

    public override void FireEvent()
    {
        if (DoStrike)
        {
            if (!strikeLoc.IsValid)
                strikeLoc = CellFinderLoose.RandomCellWith(sq => sq.Standable(map) && !map.roofGrid.Roofed(sq), map);
            boltMesh = LightningBoltMeshPool.RandomBoltMesh;
            if (strikeLoc.Fogged(map)) return;
            GenExplosion.DoExplosion(strikeLoc, map, 1.9f, DamageDefOf.Bomb, null, -1, -1f, null, null, null, null,
                null, 0f, 1, false, null, 0f, 1, 0f, false);
            var loc = strikeLoc.ToVector3Shifted();
            for (var i = 0; i < 4; i++)
            {
                FleckMaker.ThrowSmoke(loc, map, 1.5f);
                FleckMaker.ThrowMicroSparks(loc, map);
                LightningGlow(loc, map, 1.5f);
                //MoteMaker.ThrowLightningGlow(loc, this.map, 1.5f);
            }

            SoundDefOf.Thunder_OnMap.PlayOneShot(SoundInfo.InMap(new TargetInfo(strikeLoc, map, true)));
        }
        else
        {
            SoundDefOf.Thunder_OffMap.PlayOneShotOnCamera(map);
        }
    }

    private void LightningGlow(Vector3 loc, Map map, float size)
    {
        if (!loc.ShouldSpawnMotesAt(map) || map.moteCounter.SaturatedLowPriority) return;
        var moteThrown = (MoteThrown)ThingMaker.MakeThing(ThingDefOf.Mote_LightningGlow);
        moteThrown.instanceColor = LightningFlashColors.sky;
        moteThrown.Scale = Rand.Range(4f, 6f) * size;
        moteThrown.rotationRate = Rand.Range(-3f, 3f);
        moteThrown.exactPosition = loc + size * new Vector3(Rand.Value - 0.5f, 0f, Rand.Value - 0.5f);
        moteThrown.SetVelocity(Rand.Range(0, 360), 1.2f);
        GenSpawn.Spawn(moteThrown, loc.ToIntVec3(), map);
    }

    public override void WeatherEventTick()
    {
        ageTicks++;
    }

    public override void WeatherEventDraw()
    {
        UnityEngine.Graphics.DrawMesh(boltMesh, strikeLoc.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather),
            Quaternion.identity, FadedMaterialPool.FadedVersionOf(TiberiumContent.IonLightningMat, LightningBrightness),
            0);
    }
}