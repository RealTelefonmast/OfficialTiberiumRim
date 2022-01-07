using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class SpriteSheetEditor : UIElement, IDragAndDropReceiver
    {
        private SpriteSheetViewer spriteViewer;

        private Texture texture;
        private List<SpriteTile> tiles;

        private TextureSpriteSheet finalSheet;

        //
        private SpriteTile? selTile;
        private int selIndex;

        //
        private Vector2 spriteListScrollPos;
        private Rect? currentTile;


        public Texture Texture => texture;
        public List<SpriteTile> Tiles => tiles;

        //
        private static readonly Vector2 _ListSize = new Vector2(75, 20);
        private Rect BaseRect => Rect.BottomPartPixels(Rect.height-TopRect.height);
        private Rect TopPartRect => BaseRect.TopPart(.85f);
        private Rect BottomRect => BaseRect.BottomPart(.15f);
        //Work Area
        private Rect CanvasRect => TopPartRect.LeftPartPixels(TopPartRect.height).ContractedBy(10f);
        private Rect TileDataRect => TopPartRect.RightPartPixels(TopPartRect.width - TopPartRect.height).ContractedBy(10f);
        private Rect TileListRect => TileDataRect.LeftPartPixels(_ListSize.x);
        private Rect TileInfoRect => TileDataRect.RightPartPixels(TileDataRect.height);

        //Settings
        private Rect SettingsRect => BottomRect.ContractedBy(10f);
        private Rect ExportInteractRect => SettingsRect.RightPartPixels((SettingsRect.height * 1.5f) + 5);
        private Rect TileReadoutRect => SettingsRect.LeftPartPixels(SettingsRect.width - (ExportInteractRect.width + 5));
        private Rect ExportRect => ExportInteractRect.LeftPartPixels(SettingsRect.height);
        private Rect ViewRectButton => ExportInteractRect.RightPartPixels(SettingsRect.height * 0.5f);


        //Viewer
        private bool ViewerIsActive { get; set; }
        private Rect ViewerRect => new Rect(BaseRect.x, BaseRect.yMax, BaseRect.width, 250);

        //
        public override string Label => "Sprite Sheet Editor";

        public static void DrawSpriteSheet(Vector2 topLeft, TextureSpriteSheet sheet)
        {

        }

        //
        public SpriteSheetEditor()
        {
            Title = "Texture Sheet Editor";
            Size = new Vector2(800, 425);

            spriteViewer = new SpriteSheetViewer();
            tiles = new List<SpriteTile>();

            UIDragNDropper.RegisterAcceptor(this);
        }

        //
        public void LoadTexture(Texture texture)
        {
            //TODO: Notify replacemenet
            this.texture = texture;
        }

        private void CreateTile(Rect tileRect)
        {
            finalSheet ??= new TextureSpriteSheet(texture, tiles);

            var tile = new SpriteTile(CanvasRect, tileRect, Texture);
            tiles.Add(tile);
            selIndex = tiles.Count - 1;
            selTile = tiles[selIndex];
        }

        private void Clear()
        {
            tiles.Clear();
            texture = null;
            finalSheet = null;
        }

        //
        protected override void HandleEvent_Custom(Event ev, bool inContext)
        {
            var inEditor = CanvasRect.Contains(ev.mousePosition);
            if (finalSheet != null && ExportRect.Contains(ev.mousePosition))
            {
                if(ev.type == EventType.MouseDown)
                    UIDragNDropper.Notify_DraggingData(this, finalSheet, ev);
            }

            if (inEditor && CanvasRect.Contains(StartDragPos))
            {
                if (Tiles.Any(t => t.rect.Contains(ev.mousePosition)))
                {
                    
                }
                else
                {
                    var translatedPos = (StartDragPos - CanvasRect.position);

                    if (CurrentDragDiff.x < 0)
                        translatedPos.x += CurrentDragDiff.x;

                    if (CurrentDragDiff.y < 0)
                        translatedPos.y += CurrentDragDiff.y;

                    currentTile = new Rect(translatedPos, CurrentDragDiff.Abs());
                }

                if (ev.type == EventType.MouseUp)
                {
                    if (inEditor && currentTile != null)
                        CreateTile(currentTile.Value);
                    currentTile = null;
                }
            }
        }

        protected override void DrawContents(Rect inRect)
        {
            DrawCanvas(CanvasRect);

            //
            if(ViewerIsActive)
                spriteViewer.DrawElement(ViewerRect);

            if (texture == null) return;
            DrawTileList(TileListRect);
            DrawSettings(SettingsRect);

            if (selTile == null) return;
            DrawTileInfo(TileInfoRect, selTile.Value);
        }

        private void DrawCanvas(Rect rect)
        {
            TRWidgets.DrawColoredBox(rect, TRMats.BGDarker, TRMats.MenuSectionBGBorderColor, 1);
            TRWidgets.DrawGrid(rect, 10, asCellAmount:true);

            if (Texture == null) return;

            //Draw Texture
            Widgets.DrawTextureFitted(rect, Texture, 1);

            //Draw Tiles
            GUI.BeginGroup(rect);
            for (var i = 0; i < Tiles.Count; i++)
            {
                var spriteTile = Tiles[i];
                var color = selIndex == i ? Color.cyan : Color.red;
                TRWidgets.DrawBox(spriteTile.rect, color, 1);

                Widgets.Label(spriteTile.rect.ContractedBy(1), $"[{i}]");
            }

            if (currentTile != null)
                TRWidgets.DrawBox(currentTile.Value, Color.green, 1);

            if (Widgets.ButtonImage(new Rect(rect.width - 25, rect.height - 25, 25, 25), TiberiumContent.LockOpen, false))
            {
                Clear();
            }

            GUI.EndGroup();
        }

        private void DrawTileList(Rect rect)
        {
            TRWidgets.DrawColoredBox(rect, TRMats.BGDarker, TRMats.MenuSectionBGBorderColor, 1);

            GUI.BeginGroup(rect);
            rect = rect.AtZero();

            Rect fullRect = new Rect(0, 0, rect.width, Tiles.Count * _ListSize.y);
            Widgets.BeginScrollView(rect, ref spriteListScrollPos, fullRect, false);
            float curY = 0;
            for (var i = 0; i < Tiles.Count; i++)
            {
                var tile = Tiles[i];
                Rect tileRect = new Rect(0, curY, rect.width, _ListSize.y);
                var color = selIndex == i ? Color.cyan : Color.white;
                TRWidgets.DrawBox(tileRect, color, 1);
                Widgets.Label(tileRect, $"[{i}]");
                curY += _ListSize.y;
                if (Widgets.ButtonInvisible(tileRect))
                {
                    selTile = tile;
                    selIndex = i;
                }
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawTileInfo(Rect rect, SpriteTile tile)
        {
            TRWidgets.DrawColoredBox(rect, TRMats.BGLighter, TRMats.MenuSectionBGBorderColor, 1);

            GUI.BeginGroup(rect);
            rect = rect.AtZero();
            var center = rect.center;
            var rect2 = center.RectOnPos(tile.rect);

            tile.DrawTile(rect2);
            Widgets.DrawBox(rect2, 1);

            GUI.EndGroup();
        }

        private void DrawSettings(Rect rect)
        {
            var sheetExists = finalSheet != null;
            Widgets.DrawLine(new(rect.x, rect.y - 10f), new(rect.xMax, rect.y - 10), TRMats.White025, 1);

            //Readout
            Widgets.DrawBoxSolid(TileReadoutRect, TRMats.BGDarker);

            float width = rect.width;
            float size = 24;

            float height = ((float)(Math.Round((Tiles.Count / 4f), 0, MidpointRounding.AwayFromZero)) * size);

            GUI.BeginGroup(TileReadoutRect);
            Vector2 XY = Vector2.zero;
            for (var i = 0; i < tiles.Count; i++)
            {
                var sFlag = i % 2 != 0;
                var tile = tiles[i];
                Rect spriteRect = new Rect(XY, new Vector2(size, size));

                if(sFlag)
                    Widgets.DrawHighlight(spriteRect);

                tile.DrawTile(spriteRect);

                if (Mouse.IsOver(spriteRect))
                {
                    DragAndDropData = tile;
                    TRWidgets.DrawBox(spriteRect, TRMats.White05, 1);
                }

                if (XY.x + (size * 2) > width)
                {
                    XY.y += size;
                    XY.x = 0;
                }
                else
                {
                    XY.x += size;
                }
            }

            GUI.EndGroup();

            if (sheetExists)
            {
                finalSheet.DrawData(rect);
                //Draw DragSource
                TRWidgets.DrawColoredBox(ExportRect, TRMats.BGDarker, TRMats.MenuSectionBGBorderColor, 1);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(ExportRect, "Take");
                Text.Anchor = default;
            }

            //
            TRWidgets.DrawColoredBox(ViewRectButton, TRMats.BGDarker, TRMats.MenuSectionBGBorderColor, 1);
            Text.Anchor = TextAnchor.MiddleCenter;
            Matrix4x4 matrix = GUI.matrix;
            UI.RotateAroundPivot(90, ViewRectButton.center);
            Widgets.Label(ViewRectButton, ">>>");
            GUI.matrix = matrix;
            Text.Anchor = default;
            if (Widgets.ButtonInvisible(ViewRectButton))
            {
                ViewerIsActive = !ViewerIsActive;
            }
        }

        //Drag N Drop
        public void DrawHoveredData(object draggedObject, Vector2 pos)
        {
            if (draggedObject is Texture texture)
            {
                var label = $"Splice '{texture.name}'";
                var size = Text.CalcSize(label);
                pos.y -= size.y;
                Widgets.Label(new Rect(pos, size), label);
            }
        }

        public bool TryAccept(object draggedObject, Vector2 pos)
        {
            if (draggedObject is Texture texture)
            {
                LoadTexture(texture);
                return true;
            }
            return false;
        }

        public bool Accepts(object draggedObject)
        {
            return draggedObject is Texture;
        }
    }
}
