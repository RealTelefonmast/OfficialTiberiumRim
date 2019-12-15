using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public interface IUpgradable
    {
        string[] Upgrades { get; }

        void Notify_ReceivedUpgrade();

        void ReceiveUpgrade(string upgrade);
        bool HasUpgrade(string upgrade);
    }

    public class Comp_Upgradable : ThingComp
    {
        

        public CompProperties_Upgrade Props
        {
            get
            {
                return this.props as CompProperties_Upgrade;
            }
        }

        public override void CompTick()
        {
            if (Props.requisites != null)
            {

            }
        }

        public override void CompTickRare()
        {
            if (Props.requisites != null)
            {

            }
            base.CompTickRare();
        }
    }

    public class CompProperties_Upgrade : CompProperties
    {
        public Requisites requisites;
        public List<string> upgrades;
    }
}