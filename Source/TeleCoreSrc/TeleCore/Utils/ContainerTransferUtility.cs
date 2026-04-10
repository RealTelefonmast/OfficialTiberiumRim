namespace TeleCore.Utils;

/*
public class ContainerTransferUtility
{
    public const float MIN_EQ_VAL = 2;
    public const float MIN_FLOAT_COMPARE = 0.01f; //0.00390625F; //0.001953125F;


    public static bool NeedsEqualizing<T,V>(BaseContainer<T> containerA, BaseContainer<V> containerB, out ValueFlowDirection flow, out float diffPct) where T : FlowValueDef where V : FlowValueDef
    {
        flow = ValueFlowDirection.None;
        diffPct = 0f;

        var fromPct = containerA.StoredPercent;
        var toPct   = containerB.StoredPercent;

        diffPct = fromPct - toPct;
        flow = diffPct switch
        {
            > 0 => ValueFlowDirection.Positive,
            < 0 => ValueFlowDirection.Negative,
            _ => ValueFlowDirection.None
        };

        //diffPct = Mathf.Abs(diffPct);
        var relativeDiff = Mathf.Abs(diffPct / ((fromPct + toPct) / 2)); // relative difference calculation
        return relativeDiff >= 0.01f;
        //return Mathf.Abs(diffPct) >= MIN_FLOAT_COMPARE;
    }

    public static bool NeedsEqualizing<T>(BaseContainer<T> containerA, BaseContainer<T> containerB, out ValueFlowDirection flow, out float diffPct) where T : FlowValueDef
    {
        return NeedsEqualizing<T, T>(containerA, containerB, out flow, out diffPct);
    }

    public static bool NeedsEqualizing<T>(BaseContainer<T> containerA, BaseContainer<T> containerB, T def, float minDiffMargin, out ValueFlowDirection flow, out float diffPct) where T : FlowValueDef
    {
        flow = ValueFlowDirection.None;
        diffPct = 0f;
        //if (roomA.IsOutdoors && roomB.IsOutdoors) return false;
        var fromTotal = containerA.TotalStoredOf(def);
        var toTotal = containerB.TotalStoredOf(def);

        var fromPct = containerA.StoredPercentOf(def);
        var toPct = containerB.StoredPercentOf(def);

        var totalDiff = Mathf.Abs(fromTotal - toTotal);
        diffPct = fromPct - toPct;

        flow = diffPct switch
        {
            > 0 => ValueFlowDirection.Positive,
            < 0 => ValueFlowDirection.Negative,
            _ => ValueFlowDirection.None
        };
        diffPct = Mathf.Abs(diffPct);
        if (diffPct <= MIN_FLOAT_COMPARE) return false;
        return totalDiff >= minDiffMargin;
    }

    //
    public static void TryEqualizeAll<T, TH, TC>(TC from, TC to) where T : FlowValueDef
    where TH : IContainerHolder<T, TH, TC>
    where TC : BaseContainer<T, TH, TC>
    {
        if (!NeedsEqualizing<T>(from, to, out var flow, out var diffPct))
        {
            return;
        }

        var sender   = (flow == ValueFlowDirection.Positive ? from : to);
        var receiver = (flow == ValueFlowDirection.Positive ? to : from);

        var tempTypes = StaticListHolder<T>.RequestSet("EqualizingTempSet");
        tempTypes.AddRange(sender.AllStoredTypes);

        var smoothVal = receiver.Capacity * 0.1f * diffPct;
        foreach (var valueDef in tempTypes)
        {
            smoothVal = (smoothVal * valueDef.FlowRate) / sender.ValueStack.values.Length;
            smoothVal = receiver.GetMaxTransferRate(valueDef, smoothVal);
            _ = sender.TryTransferTo(receiver, valueDef, smoothVal, out _);
        }
        tempTypes.Clear();
    }

    public static void TryEqualize<T>(BaseContainer<T> from, BaseContainer<T> to, T valueDef) where T : FlowValueDef
    {
        if (!NeedsEqualizing(from, to, valueDef, MIN_EQ_VAL, out var flow, out var diffPct))
        {
            return;
        }

        BaseContainer<T> sender   = flow == ValueFlowDirection.Positive ? from : to;
        BaseContainer<T> receiver = flow == ValueFlowDirection.Positive ? to : from;

        //Get base transfer part
        var value = sender.TotalStoredOf(valueDef) * 0.5f;
        var flowAmount = receiver.GetMaxTransferRate(valueDef, Mathf.CeilToInt(value * diffPct * valueDef.FlowRate));

        //
        if (sender.TryTransferTo(receiver, valueDef, flowAmount, out _))
        {
            //...
        }

        if (sender.CanFullyTransferTo(receiver, valueDef, flowAmount))
        {
            if (sender.TryRemoveValue(valueDef, flowAmount, out float actualVal))
            {
                receiver.TryAddValue(valueDef, actualVal, out _);
            }
        }
    }
}
*/