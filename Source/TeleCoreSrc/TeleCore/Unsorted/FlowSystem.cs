using System;
using System.Collections.Generic;
using TeleCore.Defs;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public abstract class FlowSystem<TAttach, TVolume, TValueDef> : IDisposable
    where TValueDef : FlowValueDef
    where TVolume : FlowVolumeBase<TValueDef>
{
    private readonly List<TVolume> _volumes;
    private readonly List<FlowInterface<TAttach, TVolume, TValueDef>> _interfaces;

    private readonly Dictionary<TAttach, TVolume> _relations;
    private readonly Dictionary<TVolume, HashSet<FlowInterface<TAttach, TVolume, TValueDef>>> _connections;
    private readonly Dictionary<TwoWayKey<TAttach>, FlowInterface<TAttach, TVolume, TValueDef>> _interfaceLookUp;
    private DefValueStack<TValueDef, float> _totalStack;

    public IReadOnlyCollection<TVolume> Volumes => _volumes;
    public IReadOnlyCollection<FlowInterface<TAttach, TVolume, TValueDef>> Interfaces => _interfaces;
    public IReadOnlyDictionary<TAttach, TVolume> Relations => _relations;
    public IReadOnlyDictionary<TVolume, HashSet<FlowInterface<TAttach, TVolume, TValueDef>>> Connections => _connections;
    public IReadOnlyDictionary<TwoWayKey<TAttach>, FlowInterface<TAttach, TVolume, TValueDef>> InterfaceLookUp => _interfaceLookUp;

    public DefValueStack<TValueDef, float> TotalStack => _totalStack;
    public float TotalValue => _totalStack.TotalValue;

    public FlowSystem()
    {
        _volumes = new List<TVolume>();
        _interfaces = new List<FlowInterface<TAttach, TVolume, TValueDef>>();
        _relations = new Dictionary<TAttach, TVolume>();
        _connections = new Dictionary<TVolume, HashSet<FlowInterface<TAttach, TVolume, TValueDef>>>();
        _interfaceLookUp = new();
    }

    public void Reset()
    {   
        _volumes.Clear();
        _interfaces.Clear();
        _relations.Clear();
        _connections.Clear();
        _interfaceLookUp.Clear();
        _totalStack = new DefValueStack<TValueDef, float>();
    }
    
    public void Dispose()
    {
        _volumes.Clear();
        _interfaces.Clear();
        _relations.Clear();
        _connections.Clear();
        _interfaceLookUp.Clear();
    }

    #region System Data

    #region Public Manipulators
    
    protected abstract float GetInterfacePassThrough(TwoWayKey<TAttach> connectors);
    protected abstract TVolume CreateVolume(TAttach part);

    protected TVolume GenerateForOrGetVolume(TAttach part)
    {
        if (Relations.TryGetValue(part, out var volume))
            return volume;

        volume = CreateVolume(part);
        if (volume == null) return null;
        
        volume.FlowEvent += OnFlowBoxEvent;
        
        _volumes.Add(volume);
        AddRelation(part, volume);
        return volume;
    }

    public void TryRemoveRelatedPart(TAttach attach)
    {
        if (_relations.TryGetValue(attach, out var volume))
        {
            RemoveRelation(attach);
            _volumes.Remove(volume);
            _connections.Remove(volume);
            RemoveInterfacesWhere(x => x.From == volume || x.To == volume);
        }
        else
        {
            TLog.Warning($"Tried to remove node {attach} which was not registered.");
        }
    }
    
    public bool AddInterface(TwoWayKey<TAttach> connectors, FlowInterface<TAttach, TVolume, TValueDef> iFace)
    {
        if (_interfaceLookUp.TryAdd(connectors, iFace))
        {
            iFace.SetPassThrough(GetInterfacePassThrough(connectors));
            TryAddConnection(iFace.From, iFace);
            TryAddConnection(iFace.To, iFace);
            _interfaces.Add(iFace);
            return true;
        }
        //TLog.Warning($"Tried to add existing key: {connectors.A} -> {connectors.B}: {iFace}");
        return false;
    }

    protected void RemoveInterface(TwoWayKey<TAttach> connectors)
    {
        if (_interfaceLookUp.TryGetValue(connectors, out var iFace))
        {
            _interfaces.Remove(iFace);
            _interfaceLookUp.Remove(connectors);
            if (_connections.TryGetValue(iFace.From, out var conns))
            {
                conns.Remove(iFace);
            }
        }
    }

    protected void RemoveInterfacesWhere(Predicate<FlowInterface<TAttach, TVolume, TValueDef>> predicate)
    {
        for (int i = _interfaces.Count - 1; i >= 0; i--)
        {
            var iFace = _interfaces[i];
            if (predicate.Invoke(iFace))
            {
                _interfaces.RemoveAt(i);
                _interfaceLookUp.RemoveAll(t => t.Value == iFace);
                if (_connections.TryGetValue(iFace.From, out var conns))
                {
                    conns.Remove(iFace);
                }
                if (_connections.TryGetValue(iFace.To, out var conns2))
                {
                    conns2.Remove(iFace);
                }
            }
        }
    }
    
    #endregion

    public void RegisterCustomVolume(TVolume volume)
    {
        _volumes.Add(volume);
    }

    public void RegisterCustomRelation(TAttach attach, TVolume volume)
    {
        AddRelation(attach, volume);
    }

    private bool AddRelation(TAttach key, TVolume volume)
    {
        if (_relations.TryAdd(key, volume))
        {
            return true;
        }
        TLog.Warning($"Tried to add a duplicate relation to a flow system: {key}:{volume}");
        return false;
    }

    private bool RemoveRelation(TAttach key)
    {
        if (_relations.Remove(key, out var volume))
        {
            _volumes.Remove(volume);
            return true;
        }

        TLog.Warning($"Tried to remove a non-existent relation from a flow system: {key}");
        return false;
    }

    private void TryAddConnection(TVolume forVolume, FlowInterface<TAttach, TVolume, TValueDef> iFace)
    {
        if (_connections.TryGetValue(forVolume, out var list))
        {
            if (!list.Add(iFace))
            {
                //TLog.Warning($"Added duplicate interface between {iFace.FromPart} -> {iFace.ToPart}");
            }
            return;
        }

        _connections.Add(forVolume, new HashSet<FlowInterface<TAttach, TVolume, TValueDef>>() { iFace });
    }

    public void Notify_PassThroughChanged(TAttach instigator)
    {
        if (_connections.TryGetValue(Relations[instigator], out var interfaces))
        {
            foreach (var iFace in interfaces)
            {
                iFace.SetPassThrough(GetInterfacePassThrough((iFace.FromPart, iFace.ToPart)));
            }
        }
    }

    #endregion

    #region Events

    protected virtual void OnFlowBoxEvent(object sender, FlowEventArgs e)
    {
    }

    #endregion

    #region Update
    
    protected virtual void PreTickProcessor(int tick)
    {
    }

    public void Tick(int tick)
    {
        PreTickProcessor(tick);

        foreach (var _volume in _volumes)
        {
            _volume.PrevStack = _volume.Stack;
        }

        foreach (var conn in _interfaces)
        {
            UpdateFlow(conn);
        }

        foreach (var volume in _interfaces)
        {
            UpdateContent(volume);
        }

        foreach (var volume in _volumes)
        {
            UpdateFlowRate(volume);
        }
    }

    protected abstract DefValueStack<TValueDef, float> FlowFunc(FlowInterface<TAttach, TVolume, TValueDef> connection, DefValueStack<TValueDef, float> previous);
    protected abstract DefValueStack<TValueDef, float> ClampFunc(FlowInterface<TAttach, TVolume, TValueDef> connection, DefValueStack<TValueDef, float> flow, ClampType clampType);

    //protected abstract double FlowFunc(FlowInterface<TAttach, TVolume, TValueDef> connection, double previous);
    //protected abstract double ClampFunc(FlowInterface<TAttach, TVolume, TValueDef> connection, double flow, ClampType clampType);
    
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

        iface.NextFlow = flow;//ClampFunc(iface, flow, ClampType.FlowSpeed);
        iface.Move = flow;//ClampFunc(iface, flow, ClampType.FluidMove);
    }

    private static void UpdateContent(FlowInterface<TAttach, TVolume, TValueDef> conn)
    {
        foreach (var value in conn.Move)
        {
            var from = value > 0 ? conn.From : conn.To;
            var to = value > 0 ? conn.To : conn.From;
            var move = Mathf.Abs(value.Value);
            
            if (from.TryRemove(value.Def, move, out var result))
            {
                to.TryAdd(result.Def, result.Actual);
            }
        }
        
        //var result = conn.From.TryTake(conn.Move);
        //conn.To.TryInsert(result.Actual);
        
        //DefValueStack<TValueDef, double> res = conn.From.RemoveContent(conn.Move);
        //conn.To.AddContent(res);
        
        //TODO: Structify for: _connections[fb][i] = conn;
    }

    private void UpdateFlowRate(TVolume fb)
    {
        float fp = 0;
        float fn = 0;

        if (!_connections.TryGetValue(fb, out var conns)) return;
        foreach (var conn in conns)
        {
            Add(conn.Move.TotalValue);
        }

        fb.FlowRate = Mathf.Max(fp, fn);
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
    
    #endregion
}