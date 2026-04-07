using RimWorld;
using UnityEngine;
using Verse;

namespace TR.TiberiumProcessing
{
    public class TiberiumLeak : Filth
    {
        private Color color;

        public void Setup(Color color)
        {
            this.color = color;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref color, "color");
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override Color DrawColor
        {
            get
            {
                return color;
            }
        }

        public override Color DrawColorTwo
        {
            get
            {
                return color;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                Graphic graphic = base.Graphic;
                graphic.color = color;
                graphic.colorTwo = color;
                return graphic;
            }
        }
    }
}
