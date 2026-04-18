using Verse;

namespace TeleCore.Comps;

public class ThingComp_Sprite : ThingComp
{
    private int ticksLeft = -1;
    public CompProperties_Sprite Props => (CompProperties_Sprite)props;

    private Graphic_Sprite SpriteGraphic => parent.Graphic as Graphic_Sprite;

    private int AnimationTicks => Props.animationLength.SecondsToTicks();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        ticksLeft = AnimationTicks;
        SpriteGraphic.AddIndex(parent);
    }

    public override void PostDeSpawn(Verse.Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);
        SpriteGraphic.RemoveIndex(parent);
    }

    public override void CompTick()
    {
        base.CompTick();
        if (ticksLeft <= 0)
        {
            if (!Props.repeatSprite) return;
            ticksLeft = AnimationTicks;
        }

        SpriteGraphic.Next(parent);
        ticksLeft--;
    }
}

public class CompProperties_Sprite : Verse.CompProperties
{
    public float animationLength = 1;

    public bool repeatSprite = false;

    public CompProperties_Sprite()
    {
        compClass = typeof(ThingComp_Sprite);
    }
}