using System.Collections.Generic;
using System.Text;
using TeleCore.RimWorld.Defs;
using TeleCore.Shared;
using Verse;

namespace TeleCore.FlowCore.Utility;

internal static class NetworkBillUtility
{
    public static DefValueStack<NetworkValueDef, float> ConstructCustomCostStack(List<DefValueLoadable<CustomRecipeRatioDef, int>> list, bool isByProduct = false)
    {
        var stack = new DefValueStack<NetworkValueDef, float>();
        foreach (var defIntRef in list)
        {
            if (isByProduct)
            {
                foreach (var ratio in defIntRef.Def.byProducts)
                    stack += new DefValue<NetworkValueDef, float>(ratio.Def, ratio.Value * defIntRef.Value);
                continue;
            }

            foreach (var ratio in defIntRef.Def.inputRatio)
                stack += new DefValue<NetworkValueDef, float>(ratio.Def, ratio.Value * defIntRef.Value);
        }

        return stack;
    }

    public static DefValueStack<NetworkValueDef, float> ConstructCustomCostStack(
        IDictionary<CustomRecipeRatioDef, int> requestedAmount, bool isByProduct = false)
    {
        var stack = new DefValueStack<NetworkValueDef, float>();
        foreach (var defIntRef in requestedAmount)
        {
            if (isByProduct)
            {
                foreach (var ratio in defIntRef.Key.byProducts)
                    stack += new DefValue<NetworkValueDef, float>(ratio.Def, ratio.Value * defIntRef.Value);
                continue;
            }

            foreach (var ratio in defIntRef.Key.inputRatio)
                stack += new DefValue<NetworkValueDef, float>(ratio.Def, ratio.Value * defIntRef.Value);
        }

        return stack;
    }

    public static string CostLabel(DefValueStack<NetworkValueDef, float> values)
    {
        if (values.IsEmpty) return "N/A";
        var sb = new StringBuilder();
        sb.Append("(");
        for (var i = 0; i < values.Length; i++)
        {
            var input = values[i];
            sb.Append($"{input.Value}{input.Def.labelShort.Colorize(input.Def.valueColor)}");
            if (i + 1 < values.Length)
                sb.Append(" ");
        }

        sb.Append(")");
        return sb.ToString();
    }

    public static string CostLabel(List<DefValueLoadable<NetworkValueDef, float>> values)
    {
        if (values.NullOrEmpty()) return "N/A";
        var sb = new StringBuilder();
        sb.Append("(");
        for (var i = 0; i < values.Count; i++)
        {
            var input = values[i];
            sb.Append($"{input.Value}{input.Def.labelShort.Colorize(input.Def.valueColor)}");
            if (i + 1 < values.Count)
                sb.Append(" ");
        }

        sb.Append(")");
        return sb.ToString();
    }
}