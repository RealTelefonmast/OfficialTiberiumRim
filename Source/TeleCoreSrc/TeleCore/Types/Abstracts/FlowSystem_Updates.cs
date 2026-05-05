using System;
using TeleCore.Types.Enums;
using TeleCore.Types.Structs;

namespace TeleCore.Types.Abstracts;

public abstract partial class FlowSystem<TAttach, TVolume, TValueDef>
{
    protected abstract DefValueStack<TValueDef, float> FlowFunc(FlowInterface<TAttach, TVolume, TValueDef> connection,
        DefValueStack<TValueDef, float> previous);

    protected abstract DefValueStack<TValueDef, float> ClampFunc(FlowInterface<TAttach, TVolume, TValueDef> connection,
        DefValueStack<TValueDef, float> flow, ClampType clampType);

    protected abstract void PreTickProcessor(int tick);

    public void Tick(int tick)
    {
        PreTickProcessor(tick);

        foreach (var _volume in _volumes) _volume.PrevStack = _volume.Stack;

        foreach (var conn in _interfaces) UpdateFlow(conn);

        foreach (var volume in _interfaces) UpdateContent(volume);

        foreach (var volume in _volumes) UpdateFlowRate(volume);
    }

    private void UpdateFlow(FlowInterface<TAttach, TVolume, TValueDef> iface)
    {
        if (iface.PassPercent <= 0)
        {
            iface.NextFlow = DefValueStack<TValueDef, float>.Empty;
            iface.Move = DefValueStack<TValueDef, float>.Empty;
            return;
        }

        var flow = iface.NextFlow;
        flow = FlowFunc(iface, flow); //* iface.PassPercent;

        iface.NextFlow = flow; //ClampFunc(iface, flow, ClampType.FlowSpeed);
        iface.Move = flow; //ClampFunc(iface, flow, ClampType.FluidMove);
    }

    private static void UpdateContent(FlowInterface<TAttach, TVolume, TValueDef> conn)
    {
        foreach (var value in conn.Move)
        {
            var from = value > 0 ? conn.From : conn.To;
            var to = value > 0 ? conn.To : conn.From;
            var move = Math.Abs(value.Value);

            if (from.TryRemove(value.Def, move, out var result)) to.TryAdd(result.Def, result.Actual);
        }

        //var result = conn.From.TryTake(conn.Move);
        //conn.To.TryInsert(result.Actual);

        //DefValueStack<TValueDef, float> res = conn.From.RemoveContent(conn.Move);
        //conn.To.AddContent(res);

        //TODO: Structify for: _connections[fb][i] = conn;
    }

    private void UpdateFlowRate(TVolume fb)
    {
        float fp = 0;
        float fn = 0;

        if (!_connections.TryGetValue(fb, out var conns)) return;
        foreach (var conn in conns) Add(conn.Move.TotalValue);

        fb.FlowRate = Math.Max(fp, fn);
        return;

        void Add(float f)
        {
            if (f > 0)
                fp += f;
            else
                fn -= f;
        }
    }

    #region Manual Manipulation

    public void TransferFromTo(TAttach from, TAttach to, float percent)
    {
        var volumeFrom = Relations[from];
        var volumeTo = Relations[to];
        var rem = volumeFrom.RemoveContent(volumeFrom.TotalValue * percent);
        volumeTo.AddContent(rem);
    }

    public FlowResult<TValueDef, float> TransferFromTo(TAttach from, TAttach to, TValueDef def, float amount)
    {
        var volumeFrom = Relations[from];
        var volumeTo = Relations[to];
        return volumeFrom.TryTransfer(volumeTo, (def, amount));
    }

    #endregion
}