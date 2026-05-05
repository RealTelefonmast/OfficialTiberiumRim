using System.Collections.Generic;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class FXDefExtension : DefModExtension
{
    public bool alignToBottom = false;
    public bool? drawRotatedOverride = null;
    public bool rotateDrawSize = true;

    //public List<string> linkStrings;
    public List<DynamicTextureParameter> textureParams;
}