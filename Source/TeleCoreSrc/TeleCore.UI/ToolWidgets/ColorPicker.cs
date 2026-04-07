using System;
using TeleCore.AssetLoader;
using TeleCore.TeleUI;
using TeleCore.TeleUI.UITypes;
using UnityEngine;
using Verse;

namespace TeleCore.TeleUI.ToolWidgets;

public class ColorPicker
{
    private Material colorPickerMat;
    private Material hueMat;
    private Material satMat;
    private Material valMat;
    
    private Material redMat;
    private Material greenMat;
    private Material blueMat;
    
    //Current Color Working Values
    private Color _color;
    private ColorInt _colorInt;

    private Vector3 _HSVRaw;
    private Vector3Int _HSVInt;

    private static GUIStyle style;
    private static GUIStyle sliderStyle;
    private static GUIStyle sliderThumbStyle;
    
    private const string ContextSpace = "TeleColorPicker";

    public Color Color
    {
        get => _color;
        private set
        {
            _color = value;
            ColorChanged?.Invoke(value);
        }
    }
    
    public event Action<Color> ColorChanged;
    
    public ColorPicker()
    {
        style = new GUIStyle(GUI.skin.textField);
        style.normal.background = style.active.background = ContentFinder<Texture2D>.Get("TextFieldStyleCustom");
        style.hover.background = style.focused.background = ContentFinder<Texture2D>.Get("TextFieldStyleCustom_Sel");
        style.border = new RectOffset(2, 2, 2, 2);
        
        sliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
        var subSt = sliderStyle.normal;
        subSt.background = null;
        sliderStyle.normal = sliderStyle.active = sliderStyle.focused = sliderStyle.hover = subSt;
        sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
        var sliderTex = ContentFinder<Texture2D>.Get("HorizontalSliderThumb");
        sliderThumbStyle.normal.background = sliderThumbStyle.active.background = sliderThumbStyle.focused.background = sliderThumbStyle.hover.background = sliderTex;
        
        colorPickerMat = new Material(AssetBundleDB.LoadShader("ColorPickerGradient"));
        hueMat = new Material(AssetBundleDB.LoadShader("ColorPickerHue"));
        satMat = new Material(AssetBundleDB.LoadShader("ColorPickerHue"));
        valMat = new Material(AssetBundleDB.LoadShader("ColorPickerHue"));
        
        redMat = new Material(AssetBundleDB.LoadShader("ColorPickerRGB"));
        greenMat = new Material(AssetBundleDB.LoadShader("ColorPickerRGB"));
        blueMat = new Material(AssetBundleDB.LoadShader("ColorPickerRGB"));
        
        hueMat.SetInt("_Invert", 1);
        satMat.SetInt("_Invert", 1);
        valMat.SetInt("_Invert", 1);
        satMat.SetInt("_Mode", 1);
        valMat.SetInt("_Mode", 2);
        
        greenMat.SetInt("_Result", 1);
        blueMat.SetInt("_Result", 2);
    }
    
    const float padding = 5;
    const float mid_divide = 10;
    private static Vector2 _defaultSize;
    
    public static Vector2 DefaultSize => _defaultSize;
    
    static ColorPicker()
    {
        _defaultSize = GetSize(out _, out _, 0);
    }
    
    private const string string_H = "H:";
    private const string string_S = "S:";
    private const string string_V = "V:";
    private const string string_R = "R:";
    private const string string_G = "G:";
    private const string string_B = "B:";

    private static Vector2 GetSize(out float sizeSquare, out float selectorWidth, float widthOverride = 0)
    {
        var labelSize = Text.CalcSize(string_H);
        var inputSize = Text.CalcSize("1000");
        
        sizeSquare = labelSize.y * 8 + mid_divide + padding * 7;
        selectorWidth = Mathf.Max(labelSize.x + inputSize.x * 3 + padding + inputSize.x, widthOverride);
        var fullWidth = sizeSquare + mid_divide + selectorWidth;
        return new Vector2(fullWidth, sizeSquare);
    }

    private static string ToHex(Color color)
    {
        var r = (int)(color.r * 255);
        var g = (int)(color.g * 255);
        var b = (int)(color.b * 255);
        return $"{r:X2}{g:X2}{b:X2}";
    }

    private static Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out var color);
        return color;
    }
    
    public Rect Draw(Vector2 pos, float extraWidth = 0)
    {
        Rect layout;

        var labelSize = Text.CalcSize(string_H);
        var partSize = labelSize.y;
        
        //Rect stuff
        var size = GetSize(out var fullHeight, out var minWidth, extraWidth);
        var colorPicker = new Rect(pos, new Vector2(fullHeight, fullHeight));
        var rightSide = new Rect(pos.x + fullHeight + mid_divide, pos.y, minWidth, fullHeight);
        
        layout = new Rect(pos, new Vector2(size.x, size.y));
        
        //
        var div = new RectDivider(rightSide, this.GetHashCode(), new Vector2(5, 5));
        var HSV = div.NewRow(partSize);
        var HSV_H = div.NewRow(partSize);
        var HSV_S = div.NewRow(partSize);
        var HSV_V = div.NewRow(partSize, marginOverride:10);
        var textSize = Text.CalcSize("1000").x;
        var HexSize = Text.CalcSize("#FFFFFF").x;
        
        var HSV_H_Label = HSV_H.NewCol(textSize/2);
        var HSV_S_Label = HSV_S.NewCol(textSize/2);
        var HSV_V_Label = HSV_V.NewCol(textSize/2);
        
        var HSV_H_Input = HSV_H.NewCol(textSize, HorizontalJustification.Right, 5);
        var HSV_S_Input = HSV_S.NewCol(textSize, HorizontalJustification.Right, 5);
        var HSV_V_Input = HSV_V.NewCol(textSize, HorizontalJustification.Right, 5);
        
        var RGB_R = div.NewRow(partSize);
        var RGB_G = div.NewRow(partSize);
        var RGB_B = div.NewRow(partSize);
        var RGB_HEX = div.NewRow(partSize);
        
        var RGB_R_Label = RGB_R.NewCol(textSize/2);
        var RGB_G_Label = RGB_G.NewCol(textSize/2);
        var RGB_B_Label = RGB_B.NewCol(textSize/2);
        
        var RGB_R_Input = RGB_R.NewCol(textSize, HorizontalJustification.Right, 5);
        var RGB_G_Input = RGB_G.NewCol(textSize, HorizontalJustification.Right, 5);
        var RGB_B_Input = RGB_B.NewCol(textSize, HorizontalJustification.Right, 5);
        var RGB_HEX_Input = RGB_HEX.NewCol(HexSize, HorizontalJustification.Right, 5);


        Vector2 uvPos = new Vector2(_HSVRaw.y, 1 - _HSVRaw.z);
        
        //Colors
        var color = _color;
        var colorSat = Color.HSVToRGB(_HSVRaw.x, 1, 1, true);
        
        colorPickerMat.SetFloat("_Hue", _HSVRaw.x);
        redMat.SetColor("_Color", color);
        greenMat.SetColor("_Color", color);
        blueMat.SetColor("_Color", color);
        
        satMat.SetColor("_Color", colorSat);
        valMat.SetColor("_Color", colorSat);
        
        //Color Picker
        OnSelectingInPicker(colorPicker, uvPos, color);
        
        //RGB Group
        
        Widgets.Label(RGB_R_Label, "R:");
        Widgets.Label(RGB_G_Label, "G:");
        Widgets.Label(RGB_B_Label, "B:");
        Widgets.Label(RGB_HEX, "HEX:");
        
        var red = _colorInt.r;
        var green = _colorInt.g;
        var blue = _colorInt.b;
        
        var redF = (float)red;
        var greenF = (float)green;
        var blueF = (float)blue;
        
        var rSelChanged = TWidgets.Image.DrawTextureWithSlider(RGB_R, new UIMaterial(BaseContent.BadTex, redMat), ref redF, 0, 255);
        var gSelChanged = TWidgets.Image.DrawTextureWithSlider(RGB_G, new UIMaterial(BaseContent.BadTex, greenMat), ref greenF, 0, 255);
        var bSelChanged = TWidgets.Image.DrawTextureWithSlider(RGB_B, new UIMaterial(BaseContent.BadTex, blueMat), ref blueF, 0, 255);
        
        //UI context must be changed
        if(rSelChanged || gSelChanged || bSelChanged)
        {
            red = (int)redF;
            green = (int)greenF;
            blue = (int)blueF;   
            UICache.ClearContext(ContextSpace);
        }

        var oldStyle = GUI.skin.textField;
        GUI.skin.textField = style;
        var rChanged = TWidgets.Text.TextFieldNumeric(RGB_R_Input, ref red, 0, 255, context: ContextSpace);
        var gChanged = TWidgets.Text.TextFieldNumeric(RGB_G_Input, ref green, 0, 255, context: ContextSpace);
        var bChanged = TWidgets.Text.TextFieldNumeric(RGB_B_Input, ref blue, 0, 255, context: ContextSpace);

        var hexString = ToHex(color);
        var hexChanged = TWidgets.Text.TextFieldHex(RGB_HEX_Input, out string hexValue, hexString, ContextSpace);

        if (hexChanged)
        {
            var hexColor = HexToColor(hexValue);
            SetColor(hexColor);
            ColorChanged?.Invoke(hexColor);
        }
        
        if(rChanged || gChanged || bChanged)
        {
            RGBChanged(new ColorInt(red, green, blue));
        }
        
        //HSV Group
        Widgets.Label(HSV, "HSV - RGB - HEX");
        
        Widgets.Label(HSV_H_Label, "H:");
        Widgets.Label(HSV_S_Label, "S:");
        Widgets.Label(HSV_V_Label, "V:");
        
        var hInt = _HSVInt.x;
        var sInt = _HSVInt.y;
        var vInt = _HSVInt.z;

        var hF = (float)hInt;
        var sF = (float)sInt;
        var vF = (float)vInt;
        
        var hSelChanged = TWidgets.Image.DrawTextureWithSlider(HSV_H, new UIMaterial(BaseContent.BadTex, hueMat), ref hF, 0, 360);
        var sSelChanged = TWidgets.Image.DrawTextureWithSlider(HSV_S, new UIMaterial(BaseContent.BadTex, satMat), ref sF, 0, 100);
        var vSelChanged = TWidgets.Image.DrawTextureWithSlider(HSV_V, new UIMaterial(BaseContent.BadTex, valMat), ref vF, 0, 100);
        
        if(hSelChanged || sSelChanged || vSelChanged)
        {
            hInt = (int)hF;
            sInt = (int)sF;
            vInt = (int)vF;
            UICache.ClearContext(ContextSpace);
        }
        
        var hChanged = TWidgets.Text.TextFieldNumeric(HSV_H_Input, ref hInt, 0, 360, context: ContextSpace);
        var sChanged = TWidgets.Text.TextFieldNumeric(HSV_S_Input, ref sInt, 0, 100, context: ContextSpace);
        var vChanged = TWidgets.Text.TextFieldNumeric(HSV_V_Input, ref vInt, 0, 100, context: ContextSpace);

        GUI.skin.textField = oldStyle;
        
        if(hChanged || sChanged || vChanged)
        {
            HSVChanged(hInt, sInt, vInt);
        }

        return layout;
    }

    private void OnSelectingInPicker(Rect inRect, Vector2 uvPos, Color color)
    {
        var id = GUIUtility.GetControlID(FocusType.Passive, inRect);
        var type = Event.current.GetTypeForControl(id);
        switch (type)
        {
            case EventType.Repaint:
            {
                Graphics.DrawTexture(inRect, BaseContent.BadTex, 0, 0, 0, 0, colorPickerMat);
                var pickerSize = new Vector2(7, 7);
                var posRect = new Rect(inRect.position + (uvPos * inRect.size) - (pickerSize / 2), pickerSize);
                Widgets.DrawBoxSolidWithOutline(posRect, color, Color.black);
                break;
            }
            case EventType.MouseDown:
            {
                if (inRect.Contains(Event.current.mousePosition) && Event.current.button == 0)
                    GUIUtility.hotControl = id;
                break;
            }
            case EventType.MouseUp:
            {
                if (GUIUtility.hotControl == id)
                    GUIUtility.hotControl = 0;
                break;
            }
            case EventType.MouseDrag:
            {
                if (GUIUtility.hotControl == id)
                {
                    var newPos = new Vector2(Event.current.mousePosition.x - inRect.x, Event.current.mousePosition.y - inRect.y);
                    var uv = new Vector2(newPos.x / inRect.width, 1 - (newPos.y / inRect.height));
                    var uvInt = new Vector2Int((int) (uv.x * 100), (int) (uv.y * 100));
                    HSVChanged(_HSVInt.x, uvInt.x, uvInt.y);
                    GUI.changed = true;
                    Event.current.Use();
                }
                break;
            }
        }
    }
    
    public void SetColor(Color color)
    {
        Color.RGBToHSV(color, out var h, out var s, out var v);
        
        _color = color;
        _colorInt = new ColorInt(color);
        _HSVRaw = new Vector3(h, s, v);
        _HSVInt = new Vector3Int((int) (h * 360), (int) (s * 100), (int) (v * 100));
        UICache.ClearContext(ContextSpace);
    }
    
    private void RGBChanged(ColorInt newColor)
    {
        Color = newColor.ToColor;
        _colorInt = newColor;
        Color.RGBToHSV(_color, out var h, out var s, out var v);
        _HSVRaw = new Vector3(h, s, v);
        _HSVInt = new Vector3Int((int) (h * 360), (int) (s * 100), (int) (v * 100));
        UICache.ClearContext(ContextSpace);
    }
    
    private void HSVChanged(int newH, int newS, int newV)
    {
        _HSVRaw = new Vector3(newH / 360f, newS / 100f, newV / 100f);
        _HSVInt = new Vector3Int(newH, newS, newV);
        Color = Color.HSVToRGB(_HSVRaw.x, _HSVRaw.y, _HSVRaw.z);
        _colorInt = new ColorInt(_color);
        UICache.ClearContext(ContextSpace);
    }
}