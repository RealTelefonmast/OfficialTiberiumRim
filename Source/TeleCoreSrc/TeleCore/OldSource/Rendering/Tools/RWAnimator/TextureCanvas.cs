using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using TeleCore.Rendering.UI.DynaUI;
using TeleCore.Rendering.UI.DynaUI.Editing;
using TeleCore.Static;
using UnityEngine;
using Verse;
using TText = TeleCore.Utility.TText;
using TWidgets = TeleCore.Utility.TWidgets;

namespace TeleCore.Rendering.Tools.RWAnimator;

internal class TextureCanvas : BaseCanvas
{
    private TextureLayerView layerView;

    private Vector2 partListingScrollPos = Vector2.zero;

    public TextureCanvas(UIElementMode mode) : base(mode)
    {
        layerView = new TextureLayerView(this);
        AnimationData = new AnimationMetaData(this);
        UIDragNDropper.RegisterAcceptor(this);
    }

    //Public Accessors
    public AnimationMetaData AnimationData { get; private set; }

    public TimeLineControl TimeLine { get; set; }

    public override string Label => "Canvas";
    public override UIContainerMode ContainerMode => UIContainerMode.Reverse;

    //State
    public bool Initialized => AnimationData.Initialized;
    public bool ReadyToAnimate => AnimationData.SelectedAnimationPart != null;
    public bool CanWork => Initialized && ReadyToAnimate;
    private bool CanDrawEelementProperties => ActiveTexture != null && DrawElementPropertiesSetting;

    //Settings
    public bool DrawMetaDataSetting { get; private set; } = true;
    private bool DrawElementPropertiesSetting { get; set; }

    //
    public override bool CanDoRightClickMenu => CanWork;
    public override List<UIElement> ChildElements => AnimationData.CurrentElementList;

    public TextureElement ActiveTexture => layerView.ActiveElement;
    public AnimationPartValue CurrentAnimationPart => AnimationData.SelectedAnimationPart;

    //Rects
    private Rect TextureLayerViewRect => new(Position.x + Size.x - 1, Position.y, 150, Size.y);
    private Rect DataReadoutRect => new(InRect.xMax - 250, InRect.y + 1, 250, 750);

    public Rect MetaDataViewRect => new((CanWork ? TextureLayerViewRect.xMax : TextureLayerViewRect.x) - 1, Position.y,
        500 + 1, 250);

    //Data
    public override void Reset()
    {
        base.Reset();
        AnimationData = new AnimationMetaData(this);
        layerView = new TextureLayerView(this);
    }

    public void Notify_LoadedElement(UIElement loadedElement)
    {
        loadedElement.SetProperties(this);
        NotifyCollectionChanged(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, loadedElement));
        //base.Notify_AddedElement(loadedElement);
    }

    protected override void NotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems[0] is not IKeyFramedElement framedElement) return;
                foreach (var animation in AnimationData.CurrentAnimations)
                    animation.InternalFrames.Add(framedElement, new Dictionary<int, KeyFrame>());
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems[0] is not IKeyFramedElement framedElementRemoved) return;
                foreach (var animation in AnimationData.CurrentAnimations)
                    if (animation.InternalFrames.ContainsKey(framedElementRemoved))
                        animation.InternalFrames.Remove(framedElementRemoved);
                break;
        }
    }

    public override void Notify_ElementSelected(UIElement element, int index)
    {
        AnimationData.SetElementIndex(index);
    }

    protected override void Notify_ChildElementChanged(UIElement element)
    {
        base.Notify_ChildElementChanged(element);
    }

    private void Notify_SideChanged()
    {
        layerView.Notify_SelectIndex(AnimationData.ElementIndex);
    }

    private void Notify_UpdateAllLayers(TextureElement element)
    {
        AnimationData.CurrentAnimations.ForEach(c =>
            c.InternalFrames[element]
                .TryAdd(TimeLine.CurrentFrame, new KeyFrame(element.CurrentData, TimeLine.CurrentFrame)));
    }

    //Events
    protected override void HandleEvent_OnCanvas(Event ev, bool inContext = false)
    {
        if (!CanWork) return;

        if (Mouse.IsOver(InRect))
            if (CanDrawEelementProperties && Mouse.IsOver(DataReadoutRect))
                if (ev.type == EventType.MouseDown)
                    UIEventHandler.StartFocus(this, DataReadoutRect);
    }

    //Drawing
    private void DrawChildProperties(Rect rect)
    {
        Verse.Widgets.DrawMenuSection(rect);
        var listing = new Listing_Standard();
        listing.Begin(rect.ContractedBy(4));
        listing.Label("Sub Parts");
        listing.GapLine();

        foreach (var part in ActiveTexture.SubParts) TWidgets.TextureElement(listing, part);

        listing.End();
    }

    protected override void DrawTopBarExtras(Rect topRect)
    {
        if (!Initialized) return;
        var buttonRow = new Verse.WidgetRow();
        buttonRow.Init(topRect.xMax, topRect.y, UIDirection.LeftThenDown);
        if (buttonRow.ButtonIcon(TeleContent.SettingsWheel)) DrawMetaDataSetting = !DrawMetaDataSetting;
        if (buttonRow.ButtonIcon(TeleContent.BurgerMenu)) DrawElementPropertiesSetting = !DrawElementPropertiesSetting;
    }

    protected override bool CanManipulateAt(Vector2 mousePos, Rect inRect)
    {
        if (!CanWork) return false;
        if (DrawElementPropertiesSetting && DataReadoutRect.Contains(mousePos)) return false;
        return true;
    }

    protected override void DrawOnCanvas(Rect inRect)
    {
        if (Initialized)
        {
            if (DrawMetaDataSetting) DrawAnimationMetaData();

            if (!CanWork)
            {
                var workingRect = TWidgets.RectOnPos(InRect.AtZero().center, new Vector2(260, 130)).Rounded();
                TWidgets.DrawColoredBox(workingRect, TColor.BGDarker, TColor.MenuSectionBGBorderColor, 1);

                Text.Anchor = TextAnchor.MiddleCenter;
                Verse.Widgets.Label(workingRect, Translations.AnimationStrings.MissingParts);
                Text.Anchor = default;
                return;
            }
        }
        else //Draw Animation Init
        {
            var workingRect = TWidgets.RectOnPos(Origin, new Vector2(260, 130)).Rounded();
            TWidgets.DrawColoredBox(workingRect, TColor.BGDarker, TColor.MenuSectionBGBorderColor, 1);
            workingRect = workingRect.ContractedBy(5);
            var listing = new Listing_Standard();
            listing.Begin(workingRect);

            listing.Label(Translations.AnimationStrings.NewAnimation);
            listing.GapLine();
            listing.TextFieldLabeled(Translations.AnimationStrings.DefName + ": ", ref AnimationData.defName,
                TextAnchor.MiddleLeft);

            if (listing.ButtonText(Translations.AnimationStrings.Init))
            {
                if (AnimationData.defName?.Length <= 0)
                    AnimationData.defName = "NewAnim";

                AnimationData.Notify_Init();
            }

            listing.End();
            return;
        }

        //LayerView
        layerView.DrawElement(TextureLayerViewRect);

        //
        var element = UIEventHandler.FocusedElement as UIElement;
        var texElement = element as TextureElement;
        TWidgets.DoTinyLabel(inRect,
            $"Focused: {element}[{element?.RenderLayer}]\n{Event.current.mousePosition}\n{MouseOnCanvas}\n{element?.CurrentDragDiff}" +
            $"\n{(element?.StartDragPos - texElement?.TextureRect.center ?? Vector2.zero).normalized}");

        //Sel Element
        if (ActiveTexture != null)
            TextureElement.DrawOverlay(ActiveTexture);

        CanvasCursor.Notify_TriggeredMode(ActiveTexture?.LockedInMode);

        if (CanDrawEelementProperties) DrawTexturePropertiesReadout(DataReadoutRect, ActiveTexture);
    }

    //Extra Windows
    //TODO: Encapsulate into element class
    private void DrawAnimationMetaData()
    {
        var rect = MetaDataViewRect;
        Verse.Widgets.DrawMenuSection(rect);
        var leftRect = rect.LeftPartPixels(300).ContractedBy(5).Rounded();
        var rightRect = rect.RightPartPixels(200).Rounded();
        //
        var animationOutRect = rightRect.LeftPartPixels(100).ContractedBy(5).Rounded();
        var availableSetsOutRect = rightRect.RightPartPixels(100).ContractedBy(5).Rounded();

        //
        var animationLabelRect = animationOutRect.TopPart(0.1f).Rounded();
        var animationButtonRect = animationOutRect.BottomPart(0.1f).Rounded();
        var animationListingRect = new Rect(animationOutRect.x, animationLabelRect.yMax, animationOutRect.width,
            animationOutRect.height - (animationLabelRect.height + animationButtonRect.height));

        //Available Sets By Rotation
        var availableSetsLabelRect = availableSetsOutRect.TopPart(0.1f).Rounded();
        var availableSetsButtonRect = availableSetsOutRect.BottomPart(0.1f).Rounded();
        var availableSetsListingRect = new Rect(availableSetsOutRect.x, availableSetsLabelRect.yMax,
            availableSetsOutRect.width,
            availableSetsOutRect.height - (availableSetsLabelRect.height + availableSetsButtonRect.height));

        var animationPartEditRect = leftRect.BottomPart(0.5f).Rounded();

        var leftContracted = leftRect.ContractedBy(5);
        var row = new Verse.WidgetRow(leftContracted.xMax, leftContracted.y, UIDirection.LeftThenDown);
        if (row.ButtonIcon(TeleContent.HelpIcon))
        {
        }

        var listing = new Listing_Standard();
        listing.Begin(leftContracted);

        listing.Label(Translations.AnimationStrings.Settings);
        listing.GapLine();

        Text.Anchor = TextAnchor.MiddleLeft;
        listing.TextFieldLabeled($"{Translations.AnimationStrings.DefName}: ", ref AnimationData.defName,
            TextAnchor.MiddleLeft);
        Text.Anchor = default;

        listing.End();

        TWidgets.DrawColoredBox(animationPartEditRect, TColor.BGDarker, TColor.MenuSectionBGBorderColor, 1);
        var curPart = AnimationData.SelectedAnimationPart;
        if (curPart != null)
        {
            var partListing = new Listing_Standard();
            partListing.Begin(animationPartEditRect.ContractedBy(5));

            partListing.Label($"Edit '{curPart.tag}'");
            partListing.GapLine(8);
            partListing.TextFieldLabeled("Tag:", ref curPart.tag, TextAnchor.MiddleLeft);
            partListing.TextFieldNumericLabeled("Animation Length:", ref curPart._internalSeconds,
                ref curPart._internalSecondsBuffer, anchor: TextAnchor.MiddleLeft);

            if (curPart.InternalDifference)
                if (partListing.ButtonText("Set New Length"))
                    curPart.SetFrames();

            partListing.End();
        }

        Verse.Widgets.Label(animationLabelRect, Translations.AnimationStrings.SettingsAnimations);
        var partsViewRect = new Rect(animationListingRect.x, animationListingRect.y, animationListingRect.width,
            AnimationData.CurrentAnimations.Count * 20);
        TWidgets.DrawColoredBox(animationListingRect, TColor.BlueHueBG, Color.gray, 1);
        Verse.Widgets.BeginScrollView(animationListingRect, ref partListingScrollPos, partsViewRect, false);
        {
            var curY = animationListingRect.y;
            for (var i = AnimationData.CurrentAnimations.Count - 1; i >= 0; i--)
            {
                var animationPart = AnimationData.CurrentAnimations[i];
                var partListing = new Rect(animationListingRect.x, curY, animationListingRect.width, 20);

                var color = Mouse.IsOver(partListing) || AnimationData.SelectedAnimationPart == animationPart
                    ? TColor.White025
                    : TColor.White005;

                var partSelRect = partListing.ContractedBy(2).Rounded();
                Verse.Widgets.DrawBoxSolid(partSelRect, color);
                Verse.Widgets.Label(partListing.ContractedBy(5, 0), animationPart.tag);

                //
                if (TWidgets.CloseButtonCustom(partSelRect, partSelRect.height))
                    AnimationData.Notify_RemoveAnimationPart(i);

                if (Verse.Widgets.ButtonInvisible(partListing)) AnimationData.SetAnimationPart(animationPart);
                curY += 20;
            }
        }
        Verse.Widgets.EndScrollView();

        //Add Part
        if (Verse.Widgets.ButtonText(animationButtonRect, Translations.AnimationStrings.SettingsAddPart))
        {
            var animation = AnimationData.Notify_CreateNewAnimationPart("NewPart", 1);
            foreach (var element in ChildElements)
            {
                var texElement = (TextureElement) element;
                animation.InternalFrames.Add(texElement, new Dictionary<int, KeyFrame>());
                animation.InternalFrames[texElement].TryAdd(TimeLine.CurrentFrame,
                    new KeyFrame(texElement.CurrentData, TimeLine.CurrentFrame));
            }

            AnimationData.Notify_PostCreateAnimation();
        }

        Verse.Widgets.Label(availableSetsLabelRect, Translations.AnimationStrings.SettingsSets);
        TWidgets.DrawColoredBox(availableSetsListingRect, TColor.BlueHueBG, Color.gray, 1);
        var curYNew = availableSetsListingRect.y;
        for (var i = 0; i < 4; i++)
            if (AnimationData.AnimationPartsFor(i))
            {
                var setSelectionListing =
                    new Rect(availableSetsListingRect.x, curYNew, availableSetsListingRect.width, 20);

                var color = Mouse.IsOver(setSelectionListing) || AnimationData.CurRot.AsInt == i
                    ? TColor.White025
                    : TColor.White005;
                Verse.Widgets.DrawBoxSolid(setSelectionListing.ContractedBy(2).Rounded(), color);
                Verse.Widgets.Label(setSelectionListing.ContractedBy(5, 0), new Rot4(i).ToStringHuman());
                if (Verse.Widgets.ButtonInvisible(setSelectionListing))
                {
                    AnimationData.CreateOrSetRotationSet(new Rot4(i));
                    Notify_SideChanged();
                }

                curYNew += 20;
            }

        //Add Side
        if (Verse.Widgets.ButtonText(availableSetsButtonRect, Translations.AnimationStrings.SettingsAddSide))
            Find.WindowStack.Add(new FloatMenu(AddSideByRotationOptions().ToList()));
    }

    private void DrawTexturePropertiesReadout(Rect rect, TextureElement tex)
    {
        //Transform
        Verse.Widgets.DrawMenuSection(rect);
        var ev = Event.current;
        //var mousePos = ev.mousePosition;

        //
        var xSize = tex.TSize.x;
        var ySize = tex.TSize.y;
        //
        var xPos = tex.TPosition.x;
        var yPos = tex.TPosition.y;
        //
        var rot = tex.TRotation;
        //
        var xPivot = tex.PivotPoint.x;
        var yPivot = tex.PivotPoint.y;
        //TexCoords
        var xCoords = tex.TexCoords.x;
        var yCoords = tex.TexCoords.y;
        var widthCoords = tex.TexCoords.width;
        var heightCoords = tex.TexCoords.height;

        //Extra
        var attachScript = tex.AttachScript;
        var layerTag = tex.LayerTag;
        var layerIndex = tex.LayerIndex;

        //Buffer
        var buffer = tex.ValueBuffer;

        //
        var flag = ev.type == EventType.KeyDown;

        var listing = new Listing_Standard();
        listing.Begin(rect.ContractedBy(4));
        listing.Label(TText.Bold("Transform"));
        listing.GapLine();

        listing.Label("Size:");
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("X", ref xSize, ref buffer[0], SizeRange.TrueMin, SizeRange.TrueMax,
            TextAnchor.MiddleLeft);
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("Y", ref ySize, ref buffer[1], SizeRange.TrueMin, SizeRange.TrueMax,
            TextAnchor.MiddleLeft);

        listing.Label("Position:");
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("X", ref xPos, ref buffer[2], float.MinValue, anchor: TextAnchor.MiddleLeft);
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("Y", ref yPos, ref buffer[3], float.MinValue, anchor: TextAnchor.MiddleLeft);

        listing.Label("Rotation:");
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("Rot", ref rot, ref buffer[4], int.MinValue, int.MaxValue,
            TextAnchor.MiddleLeft);

        listing.GapLine();

        listing.Label("PivotPoint:");
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("X", ref xPivot, ref buffer[5], float.MinValue, anchor: TextAnchor.MiddleLeft);
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("Y", ref yPivot, ref buffer[6], float.MinValue, anchor: TextAnchor.MiddleLeft);
        listing.Label("TexCoords:");
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("X", ref xCoords, ref buffer[7], float.MinValue, anchor: TextAnchor.MiddleLeft);
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("Y", ref yCoords, ref buffer[8], float.MinValue, anchor: TextAnchor.MiddleLeft);
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("Width", ref widthCoords, ref buffer[9], float.MinValue,
            anchor: TextAnchor.MiddleLeft);
        TWidgets.DoBGForNext(listing, TColor.White025);
        listing.TextFieldNumericLabeled("Height", ref heightCoords, ref buffer[10], float.MinValue,
            anchor: TextAnchor.MiddleLeft);
        if (listing.ButtonText("Reset TexCoords")) tex.TexCoords = tex.Data.TexCoordReference;

        listing.GapLine();
        listing.Label(TText.Bold("Extra"));

        listing.TextFieldLabeled("Tag Label: ", ref layerTag, TextAnchor.MiddleLeft);

        var layerIndexBuffer = tex.Data.StringBuffer;
        listing.TextFieldNumericLabeled("Layer: ", ref layerIndex, ref layerIndexBuffer, 0, int.MaxValue,
            TextAnchor.MiddleLeft);
        tex.StringBuffer = layerIndexBuffer;

        listing.CheckboxLabeled("Attach Script: ", ref attachScript);

        if (listing.ButtonTextLabeled("TexAnchor: ", $"{tex.TexCoordAnchor}"))
        {
            var floatOptions = new List<FloatMenuOption>();
            foreach (var value in Enum.GetValues(typeof(TexCoordAnchor)))
                floatOptions.Add(new FloatMenuOption($"{value}",
                    delegate { tex.TexCoordAnchor = (TexCoordAnchor) value; }));
            Find.WindowStack.Add(new FloatMenu(floatOptions));
        }

        if (listing.ButtonTextLabeled("StretchMode: ", $"{tex.StretchMode}"))
        {
            var floatOptions = new List<FloatMenuOption>();
            foreach (var value in Enum.GetValues(typeof(TexStretchMode)))
                floatOptions.Add(
                    new FloatMenuOption($"{value}", delegate { tex.StretchMode = (TexStretchMode) value; }));
            Find.WindowStack.Add(new FloatMenu(floatOptions));
        }


        listing.GapLine();
        listing.Label(TText.Bold("View"));
        var viewTexCoords = tex.ShowTexCoordGhost;
        listing.CheckboxLabeled("View TexCoords: ", ref viewTexCoords);


        if (attachScript != tex.AttachScript) tex.AttachScript = attachScript;

        if (viewTexCoords != tex.ShowTexCoordGhost) tex.ShowTexCoordGhost = viewTexCoords;

        if (flag)
        {
            var newSize = new Vector2(xSize, ySize);
            var newPos = new Vector2(xPos, yPos);
            var newPivot = new Vector2(xPivot, yPivot);
            var newTexCoords = new Rect(xCoords, yCoords, widthCoords, heightCoords);
            //
            if (newSize != tex.TSize)
                tex.TSize = newSize;
            if (newPos != tex.TPosition)
                tex.TPosition = newPos;
            if (rot != tex.TRotation)
                tex.TRotation = rot;
            if (newPivot != tex.PivotPoint)
                tex.PivotPoint = newPivot;
            if (newTexCoords != tex.TexCoords)
                tex.TexCoords = newTexCoords;
            //
            if (layerTag != tex.LayerTag)
                tex.LayerTag = layerTag;

            if (layerIndex != tex.LayerIndex)
                tex.LayerIndex = layerIndex;
        }

        listing.End();
    }

    //Dragging
    public override void DrawHoveredData(object draggedData, Vector2 pos)
    {
        if (!CanWork) return;

        GUI.color = TColor.White05;
        if (draggedData is WrappedTexture tex)
        {
            var texture = tex.Texture;
            var drawRect = TWidgets.RectOnPos(pos, TileVector * CanvasZoomScale);
            Verse.Widgets.DrawTextureFitted(drawRect, texture, 1);
            TWidgets.DoTinyLabel(drawRect, $"{pos}");
            TWidgets.DrawBox(drawRect, Color.black, 1);
        }

        if (draggedData is SpriteTile tile)
        {
            var drawRect = TWidgets.RectOnPos(pos, tile.normalRect.size * TileVector * CanvasZoomScale);
            tile.DrawTile(drawRect);
            TWidgets.DoTinyLabel(drawRect, $"{pos}");
            TWidgets.DrawBox(drawRect, Color.black, 1);
        }

        GUI.color = Color.white;
    }

    public override bool TryAcceptDrop(object draggedObject, Vector2 pos)
    {
        if (!CanWork) return false;

        TextureElement element = null;
        if (draggedObject is WrappedTexture texture)
        {
            element = new TextureElement(new Rect(Vector2.zero, Size), texture);
            AddElement(element);
            element.SetTRSP_FromScreenSpace();

            Notify_UpdateAllLayers(element);
        }

        if (draggedObject is SpriteTile tile)
        {
            element = new TextureElement(new Rect(Vector2.zero, Size), tile.spriteMat, tile.normalRect);
            AddElement(element);
            element.SetTRSP_FromScreenSpace(pivot: tile.pivot);

            Notify_UpdateAllLayers(element);
        }

        return element != null;
    }

    public override bool CanAcceptDrop(object draggedObject)
    {
        if (!CanWork) return false;

        if (draggedObject is WrappedTexture or SpriteTile) return true;
        return false;
    }

    //
    protected override IEnumerable<FloatMenuOption> RightClickOptions()
    {
        foreach (var rightClickOption in base.RightClickOptions()) yield return rightClickOption;
    }

    private IEnumerable<FloatMenuOption> AddSideByRotationOptions()
    {
        for (var i = 0; i < 4; i++)
        {
            var rot = new Rot4(i);
            yield return new FloatMenuOption($"Rotate {rot.ToStringHuman()}", delegate
            {
                AnimationData.CreateOrSetRotationSet(rot);
                Notify_SideChanged();
            });
        }
    }
}