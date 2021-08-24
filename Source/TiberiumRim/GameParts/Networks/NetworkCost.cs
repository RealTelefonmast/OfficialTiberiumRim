using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class NetworkCost
    {
        public NetworkDef withNetwork;
        public NetworkCostSet costSet;
        public bool useDirectStorage = false;

        public NetworkCostSet Cost => costSet;

        //Validation
        public bool CanPayWith(INetworkComponent networkComponent)
        {
            return useDirectStorage ? CanPayWith(networkComponent.Container) : CanPayWith(networkComponent.Network);
        }

        private bool CanPayWith(NetworkContainer directContainer)
        {
            if (directContainer.TotalStored < Cost.TotalCost) return false;
            float totalNeeded = Cost.TotalCost;

            //
            if (Cost.HasSpecifics)
            {
                foreach (var specificCost in Cost.SpecificCosts)
                {
                    if (directContainer.ValueForType(specificCost.valueDef) >= specificCost.value)
                        totalNeeded -= specificCost.value;
                }
            }

            //
            if (Cost.mainCost > 0)
            {
                foreach (var type in Cost.AcceptedValueTypes)
                {
                    totalNeeded -= directContainer.ValueForType(type);
                }
            }

            return totalNeeded == 0;
        }

        private bool CanPayWith(Network wholeNetwork)
        {
            var totalNetworkValue = wholeNetwork.TotalNetworkValue;
            if (totalNetworkValue < Cost.TotalCost) return false;
            float totalNeeded = Cost.TotalCost;
            //Check For Specifics
            if (Cost.HasSpecifics)
            {
                foreach (var typeCost in Cost.SpecificCosts)
                {
                    var specCost = typeCost.value;
                    if (wholeNetwork.NetworkValueFor(typeCost.valueDef) >= specCost)
                        totalNeeded -= specCost;
                }
            }

            //Check For Generic Cost Value
            if (Cost.mainCost > 0)
            {
                if (wholeNetwork.TotalStorageNetworkValue >= Cost.mainCost)
                {
                    totalNeeded -= Cost.mainCost;
                }
            }

            return totalNeeded == 0;
        }

        //Process
        public void DoPayWith(Comp_NetworkStructure networkStructure)
        {
            if(useDirectStorage)
                DoPayWith(networkStructure[withNetwork].Container);
            else 
                DoPayWith(networkStructure[withNetwork].Network);
        }

        private void DoPayWith(NetworkContainer container)
        {
            var totalCost = Cost.TotalCost;
            if (totalCost <= 0) return;

            foreach (var typeCost in Cost.SpecificCosts)
            {
                if (container.TryConsume(typeCost.valueDef, typeCost.value))
                    totalCost -= typeCost.value;
            }

            foreach (var type in Cost.AcceptedValueTypes)
            {
                if (container.TryRemoveValue(type, totalCost, out float actualVal))
                {
                    totalCost -= actualVal;
                }
            }

            if (totalCost > 0)
                Log.Warning($"Paying {this} with {container.Parent} had leftOver {totalCost}");
            if (totalCost < 0)
                Log.Warning($"Paying {this} with {container.Parent} was too much: {totalCost}");
        }

        private void DoPayWith(Network network)
        {
            var totalCost = Cost.TotalCost;
            if (totalCost <= 0) return;

            foreach (var storage in network.ComponentSet.Storages.TakeWhile(storage => !(totalCost <= 0)))
            {
                foreach (var typeCost in Cost.SpecificCosts)
                {
                    if (storage.Container.TryConsume(typeCost.valueDef, typeCost.value))
                        totalCost -= typeCost.value;
                }

                foreach (var type in Cost.AcceptedValueTypes)
                {
                    if (storage.Container.TryRemoveValue(type, totalCost, out float actualVal))
                    {
                        totalCost -= actualVal;
                    }
                }
            }

            if (totalCost > 0)
                Log.Warning($"Paying {this} with {network} had leftOver: {totalCost}");
            if(totalCost < 0)
                Log.Warning($"Paying {this} with {network} was too much: {totalCost}");
        }

        public override string ToString()
        {
            return $"{Cost}|Direct: {useDirectStorage}";
        }
    }
}
