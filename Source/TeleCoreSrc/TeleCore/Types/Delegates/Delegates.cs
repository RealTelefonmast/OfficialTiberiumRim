using TeleCore.Defs;
using TeleCore.Types.Structs;

namespace TeleCore.Types.Delegates;

public delegate void NetworkVolumeStateChangedEvent<T>(VolumeChangedEventArgs<T> args) where T : FlowValueDef;

public delegate void FlowEventHandler(object sender, FlowEventArgs e);