namespace TeleCore.FlowCore.Events;

public interface INotifyFlowEvent
{
    event FlowEventHandler FlowEvent;
    void OnFlowEvent(FlowEventArgs e);
}