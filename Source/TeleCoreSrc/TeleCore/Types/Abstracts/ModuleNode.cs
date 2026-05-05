using TeleCore.UI;
using UnityEngine;
using Verse.Noise;

namespace TeleCore.Types.Abstracts;

public abstract class ModuleNode
{
    //Data

    protected ModuleNode(Vector2 position)
    {
        var module = CreateModule();
        if (module != null)
            ModuleData = new ModuleData(module, this);
        Renderer = new NodeRenderData(this, position, NodeSize);
        IOAnchors = new NodeIOData(this, Inputs, Outputs);
    }

    public ModuleData ModuleData { get; }

    public NodeRenderData Renderer { get; }

    public NodeIOData IOAnchors { get; }

    protected abstract int Inputs { get; }
    protected abstract int Outputs { get; }

    //Graphical
    public virtual Vector2 NodeSize => new(125, 200);
    public Vector2 Position => Renderer.Position;

    //
    public abstract string ModuleName { get; }

    protected abstract ModuleBase CreateModule();

    public double GetValue(double x, double y, double z)
    {
        return ModuleData.Module.GetValue(x, y, z);
    }

    public void Notify_DataChanged()
    {
        if (IOAnchors.ConnectsTo(Window_ModuleVisualizer.Vis.FinalOutput))
            Window_ModuleVisualizer.Vis.Notify_DataChanged();
    }

    public void Notify_TryConnectAt(NodeAnchor fromAnchor, Vector2 lineEnd)
    {
        Window_ModuleVisualizer.Vis.TryConnectModuleToOther(this, fromAnchor, lineEnd);
    }

    public void Notify_MouseInputAt(Event mouseEvent)
    {
        Renderer.DoMouseEvents(mouseEvent);
    }

    //
    public bool Contains(Vector2 pos)
    {
        return Renderer.Rect.Contains(pos);
    }

    public void Draw(float scale)
    {
        Renderer.DrawNode(scale);
    }

    internal bool HasInputAt(Vector2 toPos, out NodeAnchor inputAnchor)
    {
        return IOAnchors.HasInputAt(toPos, out inputAnchor);
    }

    public void Notify_NewConnection(NodeAnchor fromAnchor, NodeAnchor toAnchor)
    {
        fromAnchor.ConnectTo(toAnchor);
        toAnchor.ConnectTo(fromAnchor);
    }

    public void Notify_NewInput(int index, ModuleNode targetAnchorParentNode)
    {
        ModuleData?.Notify_SetNewInput(index, targetAnchorParentNode);
    }
}