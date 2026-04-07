using System;
using System.Collections.Generic;
using TeleCore.FlowCore.Events;
using TeleCore.Lib;
using TeleCore.Loader;
using TeleCore.Shared;
using Verse;

namespace TeleCore.FlowCore;

public abstract partial class FlowSystem<TAttach, TVolume, TValueDef> : IDisposable
    where TValueDef : FlowValueDef
    where TVolume : FlowVolumeBase<TValueDef>
{

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
}