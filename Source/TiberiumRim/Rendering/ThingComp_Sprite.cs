using Verse;

namespace TR
{
    public class ThingComp_Sprite : ThingComp
    {
        public CompProperties_Sprite Props => (CompProperties_Sprite)this.props;

        private  Graphic_Sprite SpriteGraphic => parent.Graphic as Graphic_Sprite;

        private int ticksLeft = -1;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ticksLeft = AnimationTicks;
            SpriteGraphic.AddIndex(parent);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
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

        private int AnimationTicks => Props.animationLength.SecondsToTicks();

    }

    public class CompProperties_Sprite : CompProperties
    {
        public CompProperties_Sprite()
        {
            compClass = typeof(ThingComp_Sprite);
        }

        public bool repeatSprite = false;
        public float animationLength = 1;
    }
}
