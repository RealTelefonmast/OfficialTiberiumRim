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
        

        public CompProperties_Upgrade Props => this.props as CompProperties_Upgrade;

        public IUpgradable IParent => parent as IUpgradable;

        public override void CompTick()
        {
            base.CompTick();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
        }

        public virtual void ApplyUpgrade()
        {

        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action
            {
                defaultLabel = "DEBUG: Apply Upgrades",
                action = ApplyUpgrade
            };
        }
    }

    public class CompProperties_Upgrade : CompProperties
    {
        public List<string> upgrades;
    }
}