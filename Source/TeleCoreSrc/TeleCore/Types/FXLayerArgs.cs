using System;

namespace TeleCore.Types;

public class FXArgs : EventArgs
{
    public string categoryTag;
    public int index;
    public string layerTag;
    public bool needsPower;
}

public class FXEffecterArgs : FXArgs
{
    public FXEffecterData data;
}

public class FXLayerArgs : FXArgs
{
    public FXLayerData data;
    public int renderPriority;

    public static implicit operator int(FXLayerArgs args)
    {
        return args.index;
    }

    public static implicit operator string(FXLayerArgs args)
    {
        return args.layerTag;
    }
}