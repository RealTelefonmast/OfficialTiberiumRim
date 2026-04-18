namespace TeleCore.Unsorted;

public abstract class DebugSettingWorker
{
    public abstract bool IsActive { get; }
    
    public abstract void DrawOnGUI();
    public abstract void Draw();
}