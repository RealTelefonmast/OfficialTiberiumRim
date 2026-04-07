using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TeleCore.Rendering.Tools.ModuleVisualizer;

public class ModuleData
{
    //private Traverse modulesArrayField;
    private readonly Dictionary<string, Traverse> fields = new();
    private readonly ModuleNode parent;
    private readonly string[] stringBuffers;

    public ModuleData(ModuleBase module, ModuleNode parent)
    {
        this.parent = parent;
        Module = module;
        var moduleRefl = Traverse.Create(module);
        stringBuffers = new string[moduleRefl.Fields().Count];
        var list = moduleRefl.Fields();
        for (var i = 0; i < list.Count; i++)
        {
            var field = list[i];
            var fieldRef = moduleRefl.Field(field);
            fields.Add(field, fieldRef);
            stringBuffers[i] = fieldRef.GetValue().ToString();
        }
    }

    public ModuleBase Module { get; }

    public bool HasInputs => Inputs is {Length: > 0};
    public float Height { get; }

    public ModuleBase[] Inputs
    {
        get => Module.modules;
        set => Module.modules = value;
    }

    private void DoField(Vector2 pos, float width, string label, Traverse field, int index, out float lastY)
    {
        Text.Font = GameFont.Tiny;
        var labelSize = Text.CalcSize(label);
        var labelRect = new Rect(pos.x, pos.y, labelSize.x, labelSize.y);
        var fieldRect = new Rect(pos.x, pos.y + labelSize.y, width, 20);

        Verse.Widgets.Label(labelRect, label);

        var value = (float) (double) field.GetValue();
        var prevVal = value;
        Verse.Widgets.TextFieldNumeric(fieldRect, ref value, ref stringBuffers[index], 0, 100);
        field.SetValue(value);
        if (prevVal != value)
            Notify_DataChanged();

        lastY = fieldRect.yMax;
        Text.Font = default;
    }


    public void Notify_DataChanged()
    {
        parent.Notify_DataChanged();
    }

    public void Notify_SetNewInput(int index, ModuleNode node)
    {
        var arr = Inputs;
        arr[index] = node.ModuleData.Module;
        Inputs = arr;
    }

    public void SetInputModule(int index, ModuleBase module)
    {
        var arr = Inputs;
        arr[index] = module;
        Inputs = arr;
    }

    public void DrawData(Rect rect)
    {
        Verse.Widgets.BeginGroup(rect);
        if (fields.NullOrEmpty())
        {
            Verse.Widgets.EndGroup();
            return;
        }

        var pos = new Vector2(0, 0);
        for (var i = 0; i < fields.Count; i++)
        {
            var pair = fields.ElementAt(i);
            if (pair.Value.GetValue() is not double) continue;
            DoField(pos, rect.width, pair.Key, pair.Value, i, out var newY);
            pos.y = newY;
        }

        Verse.Widgets.EndGroup();
    }
}