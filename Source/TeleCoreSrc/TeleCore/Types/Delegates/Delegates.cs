using TeleCore.Defs;

namespace TeleCore.Unsorted;

public delegate void NetworkVolumeStateChangedEvent<T>(VolumeChangedEventArgs<T> args) where T : FlowValueDef;

public delegate void FlowEventHandler(object sender, FlowEventArgs e);