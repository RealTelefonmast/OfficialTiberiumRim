using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class BlossomSpore : Particle
    {
        public TiberiumProducerDef blossom;
        private TiberiumProducer parent;

        public void SporeSetup(TiberiumProducerDef blossom, TiberiumProducer parent)
        {
            this.blossom = blossom;
            this.parent = parent;
        }

        public override void FinishAction()
        {
            GenSpawn.Spawn(blossom, Position, map);
        }
    }
}
