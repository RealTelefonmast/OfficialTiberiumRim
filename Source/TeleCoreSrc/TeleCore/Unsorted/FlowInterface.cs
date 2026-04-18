using TeleCore.Defs;
using UnityEngine;

namespace TeleCore.Unsorted;

public enum InterfaceFlowMode
{
    FromTo,
    ToFrom, 
    TwoWay
}

public class FlowInterface<TAttach, TVolume, TValueDef>
where TValueDef : FlowValueDef
where TVolume : FlowVolumeBase<TValueDef>
{
    // public double NextFlow { get; set; } = 0;
    // public double PrevFlow { get; set; } = 0;
    // public double Move { get; set; } = 0;
    
    public DefValueStack<TValueDef, float> NextFlow { get; set; }
    public DefValueStack<TValueDef, float> PrevFlow { get; set; }
    public DefValueStack<TValueDef, float> Move { get; set; }
    
    
    public float FlowRate { get; set; }

    public TAttach FromPart { get; private set; }
    public TAttach ToPart { get; private set; }
    
    public TVolume From { get; private set; }
    public TVolume To { get; private set; }
    public InterfaceFlowMode Mode { get; private set; }

    #region TODO CONCEPT

    /// <summary>
    /// Defines the amount by which values from FromPart are pushed into ToPart, ignoring the pressure (Pump)
    /// </summary>
    public float PullStrength { get; private set; } = 0f;

    #endregion
    
    public float PassPercent { get; private set; } = 1f;
    
    public FlowInterface(TAttach fromPart, TAttach toPart, TVolume from, TVolume to)
    {
        FromPart = fromPart;
        ToPart = toPart;
        From = from;
        To = to;
        Mode = InterfaceFlowMode.TwoWay;
    }

    public FlowInterface(TAttach fromPart, TAttach toPart, TVolume from, TVolume to, InterfaceFlowMode mode)
    {
        FromPart = fromPart;
        ToPart = toPart;
        From = from;
        To = to;
        Mode = mode;
    }

    // public void SetDirectionBasedOnFlow(double flow)
    // {
    //     if (flow < 0)
    //     {
    //         (From, To) = (To, From);
    //     }
    // }

    public bool ShouldFlow(TVolume from, TVolume to)
    {
        return Mode switch
        {
            InterfaceFlowMode.TwoWay => true,
            InterfaceFlowMode.FromTo => from == From && to == To,
            InterfaceFlowMode.ToFrom => from == To && to == From,
            _ => false
        };
    }
    
    public TVolume Opposite(TVolume volume)
    {
        return From == volume ? To : From;
    }

    public void SetPassThrough(float percent)
    {
        if (percent is > 1f or < 0f)
        {
            TLog.Warning("Trying to set pass through percent to " + percent + " which is not in range [0, 1]");
            percent = Mathf.Ceil(percent);
        }
        PassPercent = percent;
    }

    public override string ToString()
    {
        return $"{From.FillPercent:P2} =[{Mode}][{PassPercent:P2}]=> {To.FillPercent:P2} (Prev: {PrevFlow} Next: {NextFlow})";
        return base.ToString();
    }
}