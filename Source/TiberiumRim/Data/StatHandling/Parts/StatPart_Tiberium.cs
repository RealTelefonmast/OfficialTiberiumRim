using System;
using RimWorld;
using Verse;

namespace TR;

public abstract class StatPart_Tiberium : StatPart
{
    private StatPartMode mode;

    public virtual bool IsDisabledFor(Thing thing)
    {
        throw new NotImplementedException();
    }

    public virtual float Value(StatRequest req)
    {
        throw new NotImplementedException();
    }

    public sealed override void TransformValue(StatRequest req, ref float val)
    {
        if (IsDisabledFor(req.Thing)) return;
        switch (mode)
        {
            case StatPartMode.Multiply:
                val *= Value(req);
                break;
            case StatPartMode.Offset:
                val += Value(req);
                break;
        }
    }

    public string ValueString(StatRequest req)
    {
        char symbol = mode == StatPartMode.Multiply ? 'x' : '+';
        return $"{symbol}{Value(req).ToStringPercent()}";
    }

    public virtual string Explanation(StatRequest req)
    {
        throw new NotImplementedException();
    }

    public sealed override string ExplanationPart(StatRequest req)
    {
        if(IsDisabledFor(req.Thing)) return String.Empty;
        return Explanation(req);
    }
}
