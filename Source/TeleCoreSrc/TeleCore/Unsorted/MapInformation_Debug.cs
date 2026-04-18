using System;
using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

public class MapInformation_Debug : MapInformation
{
    private readonly List<DebugSettingWorker> _debugSettings;

    public MapInformation_Debug(Verse.Map map) : base(map)
    {
        _debugSettings = new List<DebugSettingWorker>();
        foreach (var type in typeof(DebugSettingWorker).AllSubclassesNonAbstract())
        {
            var instance = (DebugSettingWorker) Activator.CreateInstance(type);
            _debugSettings.Add(instance);
        }
    }

    public override void UpdateOnGUI()
    {
        for (var i = 0; i < _debugSettings.Count; i++)
        {
            var setting = _debugSettings[i];
            if (setting.IsActive)
            {
                setting.DrawOnGUI();
            }
        }
    }

    public override void Update()
    {
        for (var i = 0; i < _debugSettings.Count; i++)
        {
            var setting = _debugSettings[i];
            if (setting.IsActive)
            {
                setting.DrawOnGUI();
            }
        }
    }
}