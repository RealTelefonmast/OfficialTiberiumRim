using TeleCore.Types.Delegates;

namespace TeleCore.Types.Interfaces;

public interface INotifyFlowEvent
{
    event FlowEventHandler FlowEvent;
    void OnFlowEvent(FlowEventArgs e);
}