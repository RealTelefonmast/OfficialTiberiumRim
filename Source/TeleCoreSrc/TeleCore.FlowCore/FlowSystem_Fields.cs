using System.Collections.Generic;
using TeleCore.Lib;
using TeleCore.Shared;

namespace TeleCore.FlowCore;

public abstract partial class FlowSystem<TAttach, TVolume, TValueDef>
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
        _interfaceLookUp = new Dictionary<TwoWayKey<TAttach>, FlowInterface<TAttach, TVolume, TValueDef>>();
    }

    public void Dispose()
    {
        _volumes.Clear();
        _interfaces.Clear();
        _relations.Clear();
        _connections.Clear();
        _interfaceLookUp.Clear();
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
}