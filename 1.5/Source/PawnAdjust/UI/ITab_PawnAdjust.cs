using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PawnAdjust
{
    public class ITab_PawnAdjust : ITab
    {
        public static readonly Vector2 WinSize = new Vector2(480f, 510f);

        public Vector2 optionsScrollPosition;
        public float optionsViewRectHeight;

        public Dictionary<string, Texture2D> cachedHeadTextures = new Dictionary<string, Texture2D>();
        public Dictionary<string, Texture2D> cachedBodyTextures = new Dictionary<string, Texture2D>();
        public Dictionary<string, Texture2D> cachedHairTextures = new Dictionary<string, Texture2D>();
        public Dictionary<string, Texture2D> cachedBeardTextures = new Dictionary<string, Texture2D>();
        public Dictionary<string, Texture2D> cachedTattooTextures = new Dictionary<string, Texture2D>();

        public QuickSearchWidget quickSearchHair = new QuickSearchWidget();
        public QuickSearchWidget quickSearchBeard = new QuickSearchWidget();
        public QuickSearchWidget quickSearchHeadTattoo = new QuickSearchWidget();
        public QuickSearchWidget quickSearchBodyTattoo = new QuickSearchWidget();

        public List<Color> allColors;

        public Dictionary<string, float> sectionHeights = new Dictionary<string, float>();

        public float GetSectionHeight(string section)
        {
            if (!sectionHeights.ContainsKey(section))
            {
                sectionHeights.Add(section, float.MaxValue);
            }
            return sectionHeights[section];
        }

        public void SetSectionHeight(string section, float value)
        {
            sectionHeights[section] = value;
        }

        public Dictionary<string, float> sectionWidths = new Dictionary<string, float>();

        public float GetSectionWidths(string section)
        {
            if (!sectionWidths.ContainsKey(section))
            {
                sectionWidths.Add(section, float.MaxValue);
            }
            return sectionWidths[section];
        }

        public void SetSectionWidths(string section, float value)
        {
            sectionWidths[section] = value;
        }

        public Dictionary<string, bool> sectionCollapsed = new Dictionary<string, bool>();

        public bool GetSectionOpen(string section)
        {
            if (!sectionCollapsed.ContainsKey(section))
            {
                sectionCollapsed.Add(section, true);
            }
            return sectionCollapsed[section];
        }

        public void SetSectionOpen(string section, bool value)
        {
            sectionCollapsed[section] = value;
        }

        public bool CanControlPawn
		{
			get
			{
				if(SelPawn == null) { return false; }
				if (SelPawn.Downed || SelPawn.InMentalState) { return false; }
				if (SelPawn.Faction != Faction.OfPlayer) { return false; }

				return true;
			}
		}

		public override bool IsVisible => base.IsVisible && SelPawn.RaceProps.Humanlike && !SelPawn.def.HasModExtension<DefModExt_HideInAdjuster>() && (CanControlPawn || DebugSettings.godMode);

        public ITab_PawnAdjust()
        {
            this.size = WinSize;
            this.labelKey = "PawnAdjust.PawnAdjust";
        }

        public override void FillTab()
        {
            Rect inRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(16f);

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

        public void DoContents(Listing_Standard listing)
        {
            listing.Label(SelPawn.Name.ToStringFull);
            listing.GapLine();
            DoSelector_HeadType(listing);
            DoSelector_BodyType(listing);
            if (PawnAdjustMod.settings.legacySubsections)
            {
                DoSelector_HairStyle(listing);
                DoSelector_BeardStyle(listing);
                if (ModLister.IdeologyInstalled)
                {
                    DoSelector_TattooStyle(listing);
                    DoSelector_BodyTattooStyle(listing);
                }
            }
            else
            {
                DoSelector_StyleItems(listing);
            }
            DoSelector_HairColors(listing);
            DoSelector_ClothingColors(listing);
            DoSelector_RoyalTitle(listing);
            DoSelector_Ideoligion(listing);
            DoSelector_QuickCheats(listing);
        }

        public void DoSelector_HeadType(Listing_Standard listing)
        {
            string selectorString = "Selector_HeadType";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_HeadType".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                List<HeadTypeDef> allHeadTypes = GetAllCompatibleHeadTypes().ToList();
                allHeadTypes.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
                SelectorArray(listing, allHeadTypes, selectorString);
            }
        }

        public void DoSelector_BodyType(Listing_Standard listing)
        {
            if (SelPawn.ageTracker.AgeBiologicalYears < 13)
            {
                return;
            }
            string selectorString = "Selector_BodyType";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_BodyType".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                List<BodyTypeDef> allBodyTypes = GetAllCompatibleBodyTypes().ToList();
                allBodyTypes.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
                SelectorArray(listing, allBodyTypes, selectorString);
            }
        }

        public void DoSelector_StyleItems(Listing_Standard listing)
        {
            string selectorString = "Selector_StyleItems";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_StyleItems".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                if (listing.ButtonTextLabeled("PawnAdjust.Selector_HairStyle".Translate(), "PawnAdjust.ChangeButton".Translate()))
                {
                    if (!Find.WindowStack.IsOpen(typeof(Window_HairItemSelector)))
                    {
                        Find.WindowStack.Add(new Window_HairItemSelector());
                    }
                }
                if (listing.ButtonTextLabeled("PawnAdjust.Selector_BeardStyle".Translate(), "PawnAdjust.ChangeButton".Translate()))
                {
                    if (!Find.WindowStack.IsOpen(typeof(Window_BeardItemSelector)))
                    {
                        Find.WindowStack.Add(new Window_BeardItemSelector());
                    }
                }
                if (listing.ButtonTextLabeled("PawnAdjust.Selector_TattooStyle".Translate(), "PawnAdjust.ChangeButton".Translate()))
                {
                    if (!Find.WindowStack.IsOpen(typeof(Window_HeadTattooSelector)))
                    {
                        Find.WindowStack.Add(new Window_HeadTattooSelector());
                    }
                }
                if (listing.ButtonTextLabeled("PawnAdjust.Selector_BodyTattooStyle".Translate(), "PawnAdjust.ChangeButton".Translate()))
                {
                    if (!Find.WindowStack.IsOpen(typeof(Window_BodyTattooSelector)))
                    {
                        Find.WindowStack.Add(new Window_BodyTattooSelector());
                    }
                }
            }
        }

        public void DoSelector_HairStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_HairStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_HairStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                Rect rect = listing.GetRect(30f);
                quickSearchHair.OnGUI(rect);
                List<HairDef> allHairStyles = GetAllCompatibleHairStyles(quickSearchHair.filter.Text).ToList();
                allHairStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
                allHairStyles.Move(HairDefOf.Bald, 0);
                SelectorArray(listing, allHairStyles, selectorString);
            }
        }

        public void DoSelector_BeardStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_BeardStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_BeardStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                Rect rect = listing.GetRect(30f);
                quickSearchBeard.OnGUI(rect);
                List<BeardDef> allBeardStyles = GetAllCompatibleBeardStyles(quickSearchBeard.filter.Text).ToList();
                allBeardStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
                allBeardStyles.Move(BeardDefOf.NoBeard, 0);
                SelectorArray(listing, allBeardStyles, selectorString);
            }
        }

        public void DoSelector_HairColors(Listing_Standard listing)
        {
            if (!ModLister.CheckIdeology("Styling station"))
            {
                return;
            }
            string selectorString = "Selector_HairColors";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_HairColors".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                Listing_Standard hairColorListing = listing.BeginSection(GetSectionHeight(selectorString));
                float outputHeight;
                ColorSelector(new Rect(hairColorListing.curX, hairColorListing.curY, hairColorListing.ColumnWidth, GetSectionHeight(selectorString)), SelPawn.story.HairColor, GetAllColors(), out outputHeight,
                    delegate (Color color)
                    {
                        SelPawn.story.HairColor = color;
                        SelPawn.drawer.renderer.SetAllGraphicsDirty();
                    });
                SetSectionHeight(selectorString, outputHeight);
                listing.EndSection(hairColorListing);
            }
        }

        public void DoSelector_TattooStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_TattooStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_TattooStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                Rect rect = listing.GetRect(30f);
                quickSearchHeadTattoo.OnGUI(rect);
                List<TattooDef> allTattooStyles = GetAllCompatibleTattooStyles(quickSearchHeadTattoo.filter.Text).ToList();
                allTattooStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
                allTattooStyles.Move(TattooDefOf.NoTattoo_Face, 0);
                SelectorArray(listing, allTattooStyles, selectorString);
            }
        }

        public void DoSelector_BodyTattooStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_BodyTattooStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_BodyTattooStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                Rect rect = listing.GetRect(30f);
                quickSearchBodyTattoo.OnGUI(rect);
                List<TattooDef> allTattooStyles = GetAllCompatibleBodyTattooStyles(quickSearchBodyTattoo.filter.Text).ToList();
                allTattooStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
                allTattooStyles.Move(TattooDefOf.NoTattoo_Body, 0);
                SelectorArray(listing, allTattooStyles, selectorString);
            }
        }

        public void DoSelector_ClothingColors(Listing_Standard listing)
        {
            if (!ModLister.CheckIdeology("Styling station"))
            {
                return;
            }
            string selectorString = "Selector_ApparelColors";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_ApparelColors".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                foreach (Apparel item in SelPawn.apparel.WornApparel)
                {
                    Listing_Standard apparelItemBox = listing.BeginSection(GetSectionHeight(selectorString));
                    float outputHeight;
                    ColorSelector(new Rect(apparelItemBox.curX, apparelItemBox.curY, apparelItemBox.ColumnWidth, GetSectionHeight(selectorString)),
                        item.DrawColor,
                        GetAllColors(),
                        out outputHeight,
                        delegate (Color color)
                        {
                            item.SetColor(color);
                            SelPawn.drawer.renderer.SetAllGraphicsDirty();
                        }, item.def.uiIcon);
                    bool flag = false;
                    if (SelPawn.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        Rect ideoRect = new Rect(apparelItemBox.curX + 30f, apparelItemBox.curY + outputHeight, 150f, 24f);
                        if (Widgets.ButtonText(ideoRect, "SetIdeoColor".Translate()))
                        {
                            item.SetColor(SelPawn.Ideo.ApparelColor);
                            SelPawn.drawer.renderer.SetAllGraphicsDirty();
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                        flag = true;
                    }
                    Pawn_StoryTracker story = SelPawn.story;
                    if (story != null && story.favoriteColor.HasValue)
                    {
                        Rect storyRect = new Rect(160f + 30f, apparelItemBox.curY + outputHeight, 150f, 24f);
                        if (Widgets.ButtonText(storyRect, "SetFavoriteColor".Translate()))
                        {
                            item.SetColor(story.favoriteColor.Value);
                            SelPawn.drawer.renderer.SetAllGraphicsDirty();
                            SoundDefOf.Tick_Low.PlayOneShotOnCamera();
                        }
                        flag = true;
                    }
                    if (flag)
                    {
                        outputHeight += 24f;
                    }
                    SetSectionHeight(selectorString, outputHeight);
                    listing.EndSection(apparelItemBox);
                    listing.Gap(8f);
                }
            }
        }

        public void DoSelector_RoyalTitle(Listing_Standard listing)
        {

        }

        public void DoSelector_Ideoligion(Listing_Standard listing)
        {

        }

        public void SelectorArray<T>(Listing_Standard listing, List<T> defList, string selectorString)
        {
            Listing_Standard innerListing = listing.BeginSection(GetSectionHeight(selectorString));
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
                    float textGap = Text.CalcHeight(curSource, innerListing.ColumnWidth);
                    Rect textRect = new Rect(abCurX, abCurY, innerListing.ColumnWidth, textGap);
                    Widgets.Label(textRect, curSource);
                    Text.Font = GameFont.Small;
                    abCurY += textGap;
                    float y = abCurY + 6f;
                    Color color = GUI.color;
                    GUI.color = color * new Color(1f, 1f, 1f, 0.4f);
                    Widgets.DrawLineHorizontal(0, y, innerListing.ColumnWidth - 18f);
                    GUI.color = color;
                    abCurY += 12f;
                }
                DrawSelector(new Rect(abCurX, abCurY, 80f, 75f), innerListing, def);
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
            SetSectionHeight(selectorString, abCurY + 80f);
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
                        if(tattooDef.tattooType == TattooType.Face)
                        {
                            SelPawn.style.faceTattoo = tattooDef;
                        }
                        else
                        {
                            SelPawn.style.bodyTattoo = tattooDef;
                        }
                    }
                    SelPawn.Drawer.renderer.SetAllGraphicsDirty();
                    PortraitsCache.SetDirty(SelPawn);
                    PortraitsCache.PortraitsCacheUpdate();
                }
            }
            else
            {
                HeadTypeDef headTypeDef = def as HeadTypeDef;
                BodyTypeDef bodyTypeDef = def as BodyTypeDef;
                Color color = Color.white;
                Rect inRect = new Rect(rect.x + ((rect.width / 2f) - (75f / 2f)), rect.y, 75f, rect.height);
                if (Mouse.IsOver(inRect))
                {
                    color = GenUI.MouseoverColor;
                }
                MouseoverSounds.DoRegion(inRect, SoundDefOf.Mouseover_Command);
                Material material = (false ? TexUI.GrayscaleGUI : null);
                GenUI.DrawTextureWithMaterial(inRect, Command.BGTex, material);
                GUI.color = color;
                if (headTypeDef != null) { DrawHeadTypeIcon(inRect, material, headTypeDef, false); }
                if (bodyTypeDef != null) { DrawBodyTypeIcon(inRect, material, bodyTypeDef, false); }
                GUI.color = Color.white;
                if (Mouse.IsOver(inRect))
                {
                    TipSignal tip = string.Format("{0}", def.label.CapitalizeFirst() ?? def.defName);
                    TooltipHandler.TipRegion(inRect, tip);
                }
                if (Widgets.ButtonInvisible(inRect))
                {
                    if (headTypeDef != null) { SelPawn.story.headType = headTypeDef; }
                    if (bodyTypeDef != null) { SelPawn.story.bodyType = bodyTypeDef; }
                    
                    SelPawn.Drawer.renderer.SetAllGraphicsDirty();
                }
            }
        }

        public void DrawHeadTypeIcon(Rect rect, Material buttonMat, HeadTypeDef def, bool disabled)
        {
            rect.position += new Vector2(rect.width, rect.height);
            if (!disabled)
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.white.SaturationChanged(0f);
            }
            rect.position += new Vector2(-rect.size.x, -rect.size.y);
            GUI.color = SelPawn.story.SkinColor;
            Widgets.DrawTextureFitted(rect, GetHeadTexture(def), 1f * 0.85f, Vector2.one, new Rect(0f, 0f, 1f, 1f), 0, buttonMat);
            rect.position += new Vector2(rect.size.x, 0f);
            GUI.color = Color.white;
        }

        public Texture2D GetHeadTexture(HeadTypeDef def)
        {
            if (!cachedHeadTextures.ContainsKey(def.defName))
            {
                cachedHeadTextures.Add(def.defName, (Texture2D)def.GetGraphic(SelPawn, SelPawn.story.SkinColor).MatSouth.mainTexture ?? BaseContent.BadTex);
            }
            return cachedHeadTextures[def.defName];
        }

        public IEnumerable<HeadTypeDef> GetAllCompatibleHeadTypes()
        {
            foreach(HeadTypeDef head in DefDatabase<HeadTypeDef>.AllDefs)
            {
                DefModExt_HideInAdjuster ext = head.GetModExtension<DefModExt_HideInAdjuster>();
                if (CanUseHeadType(head) && (ext == null || (!ext.showForRaces.NullOrEmpty() && ext.showForRaces.Contains(SelPawn.def))))
                {
                    yield return head;
                }
            }
            yield break;
        }

        public bool CanUseHeadType(HeadTypeDef head)
        {
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && !head.requiredGenes.NullOrEmpty())
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                foreach (GeneDef requiredGene in head.requiredGenes)
                {
                    if (!pawn.genes.HasGene(requiredGene))
                    {
                        return false;
                    }
                }
            }
            if (head.gender != 0)
            {
                return head.gender == pawn.gender;
            }
            return true;
        }

        public void DrawBodyTypeIcon(Rect rect, Material buttonMat, BodyTypeDef def, bool disabled)
        {
            rect.position += new Vector2(rect.width, rect.height);
            if (!disabled)
            {
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.white.SaturationChanged(0f);
            }
            rect.position += new Vector2(-rect.size.x, -rect.size.y);
            GUI.color = SelPawn.story.SkinColor;
            Widgets.DrawTextureFitted(rect, GetBodyTexture(def), 1f * 0.85f, Vector2.one, new Rect(0f, 0f, 1f, 1f), 0, buttonMat);
            rect.position += new Vector2(rect.size.x, 0f);
            GUI.color = Color.white;
        }

        public Texture2D GetBodyTexture(BodyTypeDef def)
        {
            if (!cachedBodyTextures.ContainsKey(def.defName))
            {
                cachedBodyTextures.Add(def.defName, (Texture2D)GraphicDatabase.Get<Graphic_Multi>(def.bodyNakedGraphicPath, ShaderDatabase.Cutout, new Vector2(1f, 1f), SelPawn.story.SkinColor).MatSouth.mainTexture ?? BaseContent.BadTex);
            }
            return cachedBodyTextures[def.defName];
        }

        public IEnumerable<BodyTypeDef> GetAllCompatibleBodyTypes()
        {
            foreach (BodyTypeDef body in DefDatabase<BodyTypeDef>.AllDefs)
            {
                if(body != BodyTypeDefOf.Child && body != BodyTypeDefOf.Baby && !body.HasModExtension<DefModExt_HideInAdjuster>())
                {
                    yield return body;
                }
            }
            yield break;
        }

        public IEnumerable<HairDef> GetAllCompatibleHairStyles(string filter)
        {
            foreach (HairDef hair in DefDatabase<HairDef>.AllDefs)
            {
                if (CanUseHairStyle(hair) && !hair.HasModExtension<DefModExt_HideInAdjuster>() && StyleItemUtil.FitsFilter(filter, hair))
                {
                    yield return hair;
                }
            }
            yield break;
        }

        public bool CanUseHairStyle(HairDef hair)
        {
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && hair.requiredGene != null)
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                if (!pawn.genes.HasGene(hair.requiredGene))
                {
                    return false;
                }
            }
            return pawn.genes?.StyleItemAllowed(hair) ?? true;
        }

        public IEnumerable<BeardDef> GetAllCompatibleBeardStyles(string filter)
        {
            foreach (BeardDef beard in DefDatabase<BeardDef>.AllDefs)
            {
                if (CanUseBeardStyle(beard) && !beard.HasModExtension<DefModExt_HideInAdjuster>() && StyleItemUtil.FitsFilter(filter, beard))
                {
                    yield return beard;
                }
            }
            yield break;
        }

        public bool CanUseBeardStyle(BeardDef beard)
        {
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && beard.requiredGene != null)
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                if (!pawn.genes.HasGene(beard.requiredGene))
                {
                    return false;
                }
            }
            return pawn.genes?.StyleItemAllowed(beard) ?? true;
        }

        public static void ColorSelector(Rect rect, Color color, List<Color> colors, out float height, Action<Color> action, Texture icon = null, int colorSize = 22, int colorPadding = 2)
        {
            height = 0f;
            int num = colorSize + colorPadding * 2;
            float num2 = ((icon != null) ? ((float)(colorSize * 4) + 10f) : 0f);
            int num3 = Mathf.FloorToInt((rect.width - num2 + (float)colorPadding) / (float)(num + colorPadding));
            int num4 = Mathf.CeilToInt((float)colors.Count / (float)num3);
            Widgets.BeginGroup(rect);
            Widgets.ColorSelectorIcon(new Rect(5f, 5f, colorSize * 4, colorSize * 4), icon, color);
            for (int i = 0; i < colors.Count; i++)
            {
                int num5 = i / num3;
                int num6 = i % num3;
                float num7 = ((icon != null) ? ((num2 - (float)(num * num4) - (float)colorPadding) / 2f) : 0f);
                Rect rect2 = new Rect(num2 + (float)(num6 * num) + (float)(num6 * colorPadding), num7 + (float)(num5 * num) + (float)(num5 * colorPadding), num, num);
                if (Widgets.ColorBox(rect2, ref color, colors[i], colorSize, colorPadding))
                {
                    action.Invoke(color);
                }
                height = Mathf.Max(height, rect2.yMax);
            }
            Widgets.EndGroup();
        }

        public List<Color> GetAllColors()
        {
            if (allColors == null)
            {
                allColors = new List<Color>();
                if (SelPawn.Ideo != null && !Find.IdeoManager.classicMode)
                {
                    allColors.Add(SelPawn.Ideo.ApparelColor);
                }
                if (SelPawn.story != null && !SelPawn.DevelopmentalStage.Baby() && SelPawn.story.favoriteColor.HasValue && !allColors.Any((Color c) => SelPawn.story.favoriteColor.Value.IndistinguishableFrom(c)))
                {
                    allColors.Add(SelPawn.story.favoriteColor.Value);
                }
                foreach (ColorDef colDef in DefDatabase<ColorDef>.AllDefs.Where((ColorDef x) => x.colorType == ColorType.Ideo || x.colorType == ColorType.Misc))
                {
                    if (!allColors.Any((Color x) => x.IndistinguishableFrom(colDef.color)))
                    {
                        allColors.Add(colDef.color);
                    }
                }
                allColors.SortByColor((Color x) => x);
            }
            return allColors;
        }

        public IEnumerable<TattooDef> GetAllCompatibleTattooStyles(string filter)
        {
            foreach (TattooDef headTat in DefDatabase<TattooDef>.AllDefs)
            {
                if (CanUseTattooStyle(headTat) && headTat.tattooType == TattooType.Face && !headTat.HasModExtension<DefModExt_HideInAdjuster>() && StyleItemUtil.FitsFilter(filter, headTat))
                {
                    yield return headTat;
                }
            }
            yield break;
        }

        public bool CanUseTattooStyle(TattooDef tattoo)
        {
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && tattoo.requiredGene != null)
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                if (!pawn.genes.HasGene(tattoo.requiredGene))
                {
                    return false;
                }
            }
            return pawn.genes?.StyleItemAllowed(tattoo) ?? true;
        }

        public IEnumerable<TattooDef> GetAllCompatibleBodyTattooStyles(string filter)
        {
            foreach (TattooDef bodyTat in DefDatabase<TattooDef>.AllDefs)
            {
                if (CanUseBodyTattooStyle(bodyTat) && bodyTat.tattooType == TattooType.Body && !bodyTat.HasModExtension<DefModExt_HideInAdjuster>() && StyleItemUtil.FitsFilter(filter, bodyTat))
                {
                    yield return bodyTat;
                }
            }
            yield break;
        }

        public bool CanUseBodyTattooStyle(TattooDef tattoo)
        {
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && tattoo.requiredGene != null)
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                if (!pawn.genes.HasGene(tattoo.requiredGene))
                {
                    return false;
                }
            }
            return pawn.genes?.StyleItemAllowed(tattoo) ?? true;
        }

        #region Selector: Quick Cheats

        public void DoSelector_QuickCheats(Listing_Standard listing)
        {
            if (!DebugSettings.godMode)
            {
                return;
            }
            string selectorString = "Selector_QuickCheats";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_QuickCheats".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                Listing_Standard sectionListing = listing.BeginSection(GetSectionHeight(selectorString));
                sectionListing.ColumnWidth *= 0.45f;

                DoQuickCheat_HealFully(sectionListing);
                DoQuickCheat_MakeYoung(sectionListing);
                DoQuickCheat_StripOff(sectionListing);
                sectionListing.NewColumn();
                DoQuickCheat_ResolveAllGraphics(sectionListing);
                if (ModLister.RoyaltyInstalled)
                {
                    DoQuickCheat_GivePsylink(sectionListing);
                }
                if (ModLister.BiotechInstalled)
                {
                    DoQuickCheat_GiveMechlink(sectionListing);
                }

                SetSectionHeight(selectorString, sectionListing.MaxColumnHeightSeen);
                listing.EndSection(sectionListing);
            }
        }

        public void DoQuickCheat_StripOff(Listing_Standard listing)
        {
            Pawn p = SelPawn;
            if (listing.ButtonText("Remove All Apparel"))
            {
                p.apparel.DestroyAll();
            }
        }

        public void DoQuickCheat_HealFully(Listing_Standard listing)
        {
            Pawn p = SelPawn;
            if (listing.ButtonText("Remove All Hediffs"))
            {
                p.health.RemoveAllHediffs();
            }
        }

        public void DoQuickCheat_MakeYoung(Listing_Standard listing)
        {
            Pawn p = SelPawn;
            if (listing.ButtonText("Make Young"))
            {
                p.ageTracker.AgeBiologicalTicks = 21 * 3600000;
            }
        }

        public void DoQuickCheat_GivePsylink(Listing_Standard listing)
        {
            Pawn p = SelPawn;
            if (p.HasPsylink)
            {
                return;
            }
            if (listing.ButtonText("Give Psylink"))
            {
                Hediff_Level hediff_Level = p.GetMainPsylinkSource();
                if (hediff_Level == null)
                {
                    hediff_Level = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, p, p.health.hediffSet.GetBrain()) as Hediff_Level;
                    p.health.AddHediff(hediff_Level);
                }
            }
        }

        public void DoQuickCheat_GiveMechlink(Listing_Standard listing)
        {
            Pawn p = SelPawn;
            if (MechanitorUtility.IsMechanitor(p))
            {
                return;
            }
            if (listing.ButtonText("Give Mechlink"))
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.MechlinkImplant, p, p.health.hediffSet.GetBrain());
                p.health.AddHediff(hediff);
            }
        }

        public void DoQuickCheat_ResolveAllGraphics(Listing_Standard listing)
        {
            if (listing.ButtonText("Force Resolve"))
            {
                SelPawn.Drawer.renderer.SetAllGraphicsDirty();
            }
        }

        #endregion
    }
}
