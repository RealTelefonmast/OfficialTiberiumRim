using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TeleCore
{
    public class CustomRecipePresetDef : Def
    {
        [Unsaved]
        private List<ThingDefCount> cachedResults;
        [Unsaved]
        private string costLabel;

        public List<string> tags;
        public List<DefCount<CustomRecipeRatioDef>> desiredResources;

        public List<ThingDefCount> Results => cachedResults;
        public string CostLabel => costLabel;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            cachedResults = desiredResources.Select(m => new ThingDefCount(m.Def.result, m.Value)).ToList();
            costLabel = NetworkBillUtility.CostLabel(NetworkBillUtility.ConstructCustomCost(desiredResources));

            //
            CustomNetworkRecipeReferences.TryRegister(this);
        }
    }
}
