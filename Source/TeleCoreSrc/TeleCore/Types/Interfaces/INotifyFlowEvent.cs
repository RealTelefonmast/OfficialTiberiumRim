namespace TeleCore.Unsorted;

public interface INotifyFlowEvent
{
    event FlowEventHandler FlowEvent;
    void OnFlowEvent(FlowEventArgs e);
}