using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore;

//TODO: Data Layer Seperation From Views
//TODO: Animations re-structure:
//  [Sets] AnimationSet List //Up to four set elements
//      [Rot.0] type:AnimationSet //A set is based on a rotation, and can contain multiple animations
//          [Animation.0] type:AnimationPart //One animation like "action"
//              [Texture.0] => textures are index-mirrored to another list with keyframes for that texture
//              [Texture.1]
//          [Animation.1] type:AnimationPart //Another animation like "idle"
//              [Texture.0]
//              [Texture.1]
//          [Animation.2] type:AnimationPart //Could have a special animation that uses a different amount of textures
//              [Texture.0]
//              [Texture.1]
//              [Texture.2]
//      [Rot.2]
//          [Animation.0]
internal class AnimationWindowLayout : UIElement
{
    private readonly TextureBrowser browser;

    private readonly TextureCanvas canvas;

    //
    private readonly Dialog_AnimationFileList dialogFileSaveLoading;
    private readonly SpriteSheetEditor spriteSheetEditor;

    private readonly TimeLineControl timeLine;

    //private readonly ToolBar toolBar;
    private readonly UITopBar topBar;

    private bool keepProject;
    private readonly Window parentWindow;

    public AnimationWindowLayout(Window parent, Rect rect, UIElementMode mode) : base(rect, mode)
    {
        //
        //bgColor = TColor.WindowBGFillColor;
        //borderColor = TColor.WindowBGBorderColor;

        parentWindow = parent;
        bgColor = TColor.BGDarker;
        borderColor = Color.clear;

        //
        timeLine = new TimeLineControl();
        //toolBar = new ToolBar(UIElementMode.Static);
        canvas = new TextureCanvas(UIElementMode.Static);
        canvas.TimeLine = timeLine;
        timeLine.Canvas = canvas;
        //
        spriteSheetEditor = new SpriteSheetEditor(UIElementMode.Static);
        spriteSheetEditor.SetVisibility(UIElementState.Closed);

        //
        browser = new TextureBrowser(UIElementMode.Static);
        browser.SetVisibility(UIElementState.Closed);

        dialogFileSaveLoading = new Dialog_AnimationFileList(canvas);
        //
        var buttonMenus = new List<TopBarButtonMenu>();
        //File
        var fileOptions = new List<TopBarButtonOption>();
        fileOptions.Add(new TopBarButtonOption("New", canvas.Reset));
        fileOptions.Add(new TopBarButtonOption("Save/Load", () => { Find.WindowStack.Add(dialogFileSaveLoading); }));

        buttonMenus.Add(new TopBarButtonMenu("File", fileOptions));

        //View
        var viewOptions = new List<TopBarButtonOption>();
        viewOptions.Add(new TopBarButtonOption("Texture Slicer", () => { spriteSheetEditor.ToggleOpen(); }));
        viewOptions.Add(new TopBarButtonOption("Texture Browser", () => { browser.ToggleOpen(); }));

        buttonMenus.Add(new TopBarButtonMenu("View", viewOptions));

        //Help
        buttonMenus.Add(new TopBarButtonMenu("Help", () => { Find.WindowStack.Add(new Dialog_AnimatorHelp()); }));
        topBar = new UITopBar(buttonMenus);
        topBar.AddCloseButton(() =>
        {
            keepProject = false;
            if (canvas.Initialized)
            {
                var msgBox = new Dialog_MessageBox("Do you want to keep the current project open after closing?",
                    "Yes keep it",
                    buttonBText: "No just close it",
                    buttonAAction: () =>
                    {
                        keepProject = true;
                        parentWindow.Close();
                    },
                    buttonBAction: () => { parentWindow.Close(); },
                    cancelAction: () => { },
                    layer: WindowLayer.Super
                );
                Find.WindowStack.Add(msgBox);
            }
            else
            {
                parentWindow.Close();
            }
        });
    }

    public void Notify_Reopened()
    {
        if (keepProject) return;
        canvas?.Reset();
        timeLine?.Reset();
    }

    protected override void DrawContentsBeforeRelations(Rect inRect)
    {
        var topRect = inRect.TopPart(0.80f).Rounded();
        var canvasRect = topRect.LeftPartPixels(900);
        var toolBarRect = new Rect(inRect.width - 125, inRect.y, 125, canvas.MetaDataViewRect.height);
        var timeLineRect = inRect.BottomPart(0.20f).Rounded();

        //Object Browser
        var objectBrowserRect = new Rect(canvas.MetaDataViewRect.x, canvasRect.y - 1, 300, canvasRect.height + 1);
        if (canvas.DrawMetaDataSetting && canvas.Initialized)
            objectBrowserRect = new Rect(canvas.MetaDataViewRect.x, canvas.MetaDataViewRect.yMax - 1, 300,
                canvasRect.height - canvas.MetaDataViewRect.height + 1);

        //
        var spriteSheetEditorRect = new Rect(objectBrowserRect.xMax - 1, canvas.MetaDataViewRect.yMax - 1,
            inRect.width - objectBrowserRect.xMax, topRect.height - canvas.MetaDataViewRect.height + 1);

        canvas.DrawElement(canvasRect);
        //toolBar.DrawElement(toolBarRect);
        timeLine.DrawElement(timeLineRect);

        browser.DrawElement(objectBrowserRect);
        spriteSheetEditor.DrawElement(spriteSheetEditorRect);

        //
        topBar.DrawElement(TopRect);
    }
}