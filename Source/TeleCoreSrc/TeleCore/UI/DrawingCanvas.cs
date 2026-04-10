using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class DrawingCanvas
{
    private RenderTexture WorkingTex;
    private ComputeShader DrawShader;
    
    //Note: Some test stuff for drawing on a canvas texture
    public void Draw(Rect rect)
    {
        ComputeShader compute = DrawShader;
        RenderTexture texture = WorkingTex; //A render texture that we feed into the compute shader

        var canvasRect = rect;
        
        Widgets.BeginGroup(canvasRect);
        var canvasArea = canvasRect.AtZero();
        Widgets.DrawTextureFitted(canvasArea, texture, 1);

        var mouse = Event.current.mousePosition;
        var mouseNormalized = mouse - canvasRect.position;

        var mouseDown = Event.current.isMouse && Event.current.button == 0; //Left mouse down
        compute.SetBool("_IsDrawing", mouseDown);
        if (mouseDown)
        {
            compute.SetVector("_MousePos", mouseNormalized);
            //This tells the shader where to draw on the rendertexture
        }
        
        Widgets.EndGroup();
    }
}