using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PawnAdjust
{
    public class Window_StyleItemSelector : Window
    {
        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public override void PreOpen()
        {
            base.PreOpen();
            doCloseX = true;
            doCloseButton = true;
            resizeable = true;
            draggable = true;
        }

        public Vector2 optionsScrollPosition;

        public float optionsViewRectHeight;
        public float subsectionViewHeight;

        public Pawn SelPawn => Find.Selector.SingleSelectedThing as Pawn;

        public virtual string headerLabel => "";

        public override void DoWindowContents(Rect inRect)
        {
            if(SelPawn == null)
            {
                Close();
            }
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width - 150f - 17f, 35f), headerLabel);
            Text.Font = GameFont.Small;
            Rect inRect2 = new Rect(0f, 40f, inRect.width, inRect.height - 40f - Window.CloseButSize.y);
            DoInRect(inRect2);
        }

        public void DoInRect(Rect inRect)
        {
            bool flag = optionsViewRectHeight > inRect.height;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - (flag ? 26f : 0f), optionsViewRectHeight);
            Widgets.BeginScrollView(inRect, ref optionsScrollPosition, viewRect);
            Listing_Standard listing = new Listing_Standard();
            Rect rect = new Rect(viewRect.x, viewRect.y, viewRect.width, 999999f);
            listing.Begin(rect);
            // ============================ CONTENTS ================================
            DoContents(listing);
            // ======================================================================
            optionsViewRectHeight = listing.CurHeight;
            listing.End();
            Widgets.EndScrollView();
        }

        public virtual void DoContents(Listing_Standard listing)
        {

        }

        public void SelectorArray<T>(Listing_Standard listing, List<T> defList)
        {
            Listing_Standard innerListing = listing.BeginSection(subsectionViewHeight);
            float abCurY = 0f;
            float abCurX = 0f;
            string curSource = "";
            int curCount = 0;
            for (int i = 0; i < defList.Count(); i++)
            {
                Def def = defList[i] as Def;
                if (def.modContentPack.Name != curSource)
                {
                    curSource = def.modContentPack.Name;
                    if (abCurX != 0f)
                    {
                        abCurY += 80f;
                        abCurX = 0f;
                        curCount = 0;
                    }
                    Text.Font = GameFont.Tiny;
                    GUI.color = Color.white;
                    float textGap = Text.CalcHeight(curSource, listing.ColumnWidth);
                    Rect textRect = new Rect(abCurX, abCurY, listing.ColumnWidth, textGap);
                    Widgets.Label(textRect, curSource);
                    Text.Font = GameFont.Small;
                    abCurY += textGap;
                    float y = abCurY + 6f;
                    Color color = GUI.color;
                    GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
                    Widgets.DrawLineHorizontal(0, y, listing.ColumnWidth - 18f);
                    GUI.color = color;
                    abCurY += 12f;
                }
                DrawSelector(new Rect(abCurX, abCurY, 80f, 75f), listing, def);
                // Handle Row/Column Position.
                curCount++;
                if (curCount < defList.Count)
                {
                    if (curCount % Mathf.FloorToInt(innerListing.ColumnWidth / 80f) == 0)
                    {
                        abCurY += 80f;
                        abCurX = 0f;
                        curCount = 0;
                    }
                    else
                    {
                        abCurX += 80f;
                    }
                }
            }
            subsectionViewHeight = abCurY + 80f;
            listing.EndSection(innerListing);
        }

        public void DrawSelector<T>(Rect rect, Listing_Standard listing, T inDef)
        {
            Def def = inDef as Def;
            if (def == null) { return; }
            StyleItemDef styleItemDef = def as StyleItemDef;
            if (styleItemDef != null)
            {
                Color color = Color.white;
                Rect inRect = new Rect(rect.x + ((rect.width / 2f) - (75f / 2f)), rect.y, 75f, rect.height);
                if (Mouse.IsOver(inRect))
                {
                    color = GenUI.MouseoverColor;
                }
                MouseoverSounds.DoRegion(inRect, SoundDefOf.Mouseover_Command);
                Material material = (false ? TexUI.GrayscaleGUI : null);
                GenUI.DrawTextureWithMaterial(inRect, Command.BGTex, material);
                if (styleItemDef is HairDef || styleItemDef is BeardDef)
                {
                    GUI.color = SelPawn.story.HairColor;
                }
                if (!styleItemDef.noGraphic)
                {
                    Widgets.DefIcon(inRect, styleItemDef, color: SelPawn.story.HairColor);
                }
                GUI.color = Color.white;
                if (Mouse.IsOver(inRect))
                {
                    TooltipHandler.TipRegion(inRect, styleItemDef.GetStyleTooltip());
                }
                if (Widgets.ButtonInvisible(inRect))
                {
                    HairDef hairDef = styleItemDef as HairDef;
                    if (hairDef != null)
                    {
                        SelPawn.story.hairDef = hairDef;
                    }
                    BeardDef beardDef = styleItemDef as BeardDef;
                    if (beardDef != null)
                    {
                        SelPawn.style.beardDef = beardDef;
                    }
                    TattooDef tattooDef = styleItemDef as TattooDef;
                    if (tattooDef != null)
                    {
                        if (tattooDef.tattooType == TattooType.Face)
                        {
                            SelPawn.style.faceTattoo = tattooDef;
                        }
                        else
                        {
                            SelPawn.style.bodyTattoo = tattooDef;
                        }
                    }
                    SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(SelPawn);
                    PortraitsCache.PortraitsCacheUpdate();
                }
            }
        }
    }
}
