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
            base.FinishAction();
            Log.Message("Spore arrived at: " + Position + " ... Finishing");
            if (Position.SupportsBlossom(map))
            {
                GenSpawn.Spawn(blossom, Position, map);
            }
            Log.Message("Spawned Blossom thing");
        }
    }
}
