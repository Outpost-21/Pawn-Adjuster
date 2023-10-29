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

        public Dictionary<string, float> sectionHeights = new Dictionary<string, float>();

        public List<Color> allColors;

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

		public override bool IsVisible => base.IsVisible && (CanControlPawn || DebugSettings.godMode);

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
            DoSelector_HairStyle(listing);
            DoSelector_BeardStyle(listing);
            DoSelector_TattooStyle(listing);
            DoSelector_BodyTattooStyle(listing);
            DoSelector_HairColors(listing);
            DoSelector_ClothingColors(listing);
            DoSelector_RoyalTitle(listing);
            DoSelector_Ideoligion(listing);
            DoSelector_QuickCheats(listing);
        }

        #region Selector: Head Type

        public void DoSelector_HeadType(Listing_Standard listing)
        {
            string selectorString = "Selector_HeadType";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_HeadType".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                List<HeadTypeDef> allHeadTypes = GetAllCompatibleHeadTypes().ToList();
                allHeadTypes.SortBy(gd => gd.defName);
                Listing_Standard headTypeListing = listing.BeginSection(GetSectionHeight(selectorString));
                float abCurY = 0f;
                float abCurX = 0f;
                for (int i = 0; i < allHeadTypes.Count(); i++)
                {
                    DrawHeadTypeSelector(new Rect(abCurX, abCurY, 80f, 75f), headTypeListing, allHeadTypes[i]);
                    // Handle Row/Column Position.
                    if (i + 1 < allHeadTypes.Count())
                    {
                        if ((i + 1) % 5 == 0)
                        {
                            abCurY += 80f;
                            abCurX = 0f;
                        }
                        else
                        {
                            abCurX += 80f;
                        }
                    }
                }
                SetSectionHeight(selectorString, abCurY + 80f);
                listing.EndSection(headTypeListing);
            }
        }

        public void DrawHeadTypeSelector(Rect rect, Listing_Standard listing, HeadTypeDef head)
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
            GUI.color = color;
            DrawHeadTypeIcon(inRect, material, head, false);
            GUI.color = Color.white;
            if (Mouse.IsOver(inRect))
            {
                TipSignal tip = string.Format("{0}", head.label.CapitalizeFirst() ?? head.defName);
                TooltipHandler.TipRegion(inRect, tip);
            }
            if (Widgets.ButtonInvisible(inRect))
            {
                SelPawn.story.headType = head;
                SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
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
                cachedHeadTextures.Add(def.defName, (Texture2D)def.GetGraphic(SelPawn.story.SkinColor).MatSouth.mainTexture ?? BaseContent.BadTex);
            }
            return cachedHeadTextures[def.defName];
        }

        public IEnumerable<HeadTypeDef> GetAllCompatibleHeadTypes()
        {
            foreach(HeadTypeDef head in DefDatabase<HeadTypeDef>.AllDefs)
            {
                if (CanUseHeadType(head) && !head.HasModExtension<DefModExt_HideInAdjuster>())
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

        #endregion

        #region Selector: Body Type

        public void DoSelector_BodyType(Listing_Standard listing)
        {
            if(SelPawn.ageTracker.AgeBiologicalYears < 13)
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
                allBodyTypes.SortBy(gd => gd.defName);
                Listing_Standard headTypeListing = listing.BeginSection(GetSectionHeight(selectorString));
                float abCurY = 0f;
                float abCurX = 0f;
                for (int i = 0; i < allBodyTypes.Count(); i++)
                {
                    DrawBodyTypeSelector(new Rect(abCurX, abCurY, 80f, 75f), headTypeListing, allBodyTypes[i]);
                    // Handle Row/Column Position.
                    if (i + 1 < allBodyTypes.Count())
                    {
                        if ((i + 1) % 5 == 0)
                        {
                            abCurY += 80f;
                            abCurX = 0f;
                        }
                        else
                        {
                            abCurX += 80f;
                        }
                    }
                }
                SetSectionHeight(selectorString, abCurY + 80f);
                listing.EndSection(headTypeListing);
            }
        }

        public void DrawBodyTypeSelector(Rect rect, Listing_Standard listing, BodyTypeDef body)
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
            GUI.color = color;
            DrawBodyTypeIcon(inRect, material, body, false);
            GUI.color = Color.white;
            if (Mouse.IsOver(inRect))
            {
                TipSignal tip = string.Format("{0}", body.label.CapitalizeFirst() ?? body.defName);
                TooltipHandler.TipRegion(inRect, tip);
            }
            if (Widgets.ButtonInvisible(inRect))
            {
                SelPawn.story.bodyType = body;
                SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
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

        #endregion

        #region Selector: Hair Style

        public void DoSelector_HairStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_HairStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_HairStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                List<HairDef> allHairStyles = GetAllCompatibleHairStyles().ToList();
                allHairStyles.SortBy(gd => gd.defName);
                Listing_Standard headTypeListing = listing.BeginSection(GetSectionHeight(selectorString));
                float abCurY = 0f;
                float abCurX = 0f;
                for (int i = 0; i < allHairStyles.Count(); i++)
                {
                    DrawHairStyleSelector(new Rect(abCurX, abCurY, 80f, 75f), headTypeListing, allHairStyles[i]);
                    // Handle Row/Column Position.
                    if (i + 1 < allHairStyles.Count())
                    {
                        if ((i + 1) % 5 == 0)
                        {
                            abCurY += 80f;
                            abCurX = 0f;
                        }
                        else
                        {
                            abCurX += 80f;
                        }
                    }
                }
                SetSectionHeight(selectorString, abCurY + 80f);
                listing.EndSection(headTypeListing);
            }
        }

        public void DrawHairStyleSelector(Rect rect, Listing_Standard listing, HairDef hair)
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
            GUI.color = SelPawn.story.HairColor;
            Widgets.DefIcon(inRect, hair, color: SelPawn.story.HairColor);
            GUI.color = Color.white;
            if (Mouse.IsOver(inRect))
            {
                TooltipHandler.TipRegion(inRect, GetHairTooltip(hair));
            }
            if (Widgets.ButtonInvisible(inRect))
            {
                SelPawn.story.hairDef = hair;
                SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }

        public TipSignal GetHairTooltip(HairDef def)
        {
            TipSignal tip = string.Format("{0}", def.label.CapitalizeFirst() ?? def.defName);
            if (!def.styleTags.NullOrEmpty())
            {
                tip += ("\n" + "PawnAdjust.StyleTags".Translate() + def.styleTags.ToString()).Colorize(Color.grey);
            }
            if (def.modContentPack != null && def.modContentPack.Name != null)
            {
                tip += "\n" + "PawnAdjust.ModSource".Translate() + def.modContentPack.Name.Colorize(Color.blue);
            }
            return tip;
        }

        public IEnumerable<HairDef> GetAllCompatibleHairStyles()
        {
            foreach (HairDef hair in DefDatabase<HairDef>.AllDefs)
            {
                if (CanUseHairStyle(hair) && !hair.HasModExtension<DefModExt_HideInAdjuster>() && !hair.texPath.NullOrEmpty())
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
            return true;
        }

        #endregion

        #region Selector: Beard Style

        public void DoSelector_BeardStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_BeardStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_BeardStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                List<BeardDef> allBeardStyles = GetAllCompatibleBeardStyles().ToList();
                allBeardStyles.SortBy(gd => gd.defName);
                Listing_Standard headTypeListing = listing.BeginSection(GetSectionHeight(selectorString));
                float abCurY = 0f;
                float abCurX = 0f;
                for (int i = 0; i < allBeardStyles.Count(); i++)
                {
                    DrawBeardStyleSelector(new Rect(abCurX, abCurY, 80f, 75f), headTypeListing, allBeardStyles[i]);
                    // Handle Row/Column Position.
                    if (i + 1 < allBeardStyles.Count())
                    {
                        if ((i + 1) % 5 == 0)
                        {
                            abCurY += 80f;
                            abCurX = 0f;
                        }
                        else
                        {
                            abCurX += 80f;
                        }
                    }
                }
                SetSectionHeight(selectorString, abCurY + 80f);
                listing.EndSection(headTypeListing);
            }
        }

        public void DrawBeardStyleSelector(Rect rect, Listing_Standard listing, BeardDef beard)
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
            GUI.color = SelPawn.story.HairColor;
            Widgets.DefIcon(inRect, beard, color: SelPawn.story.HairColor);
            GUI.color = Color.white;
            if (Mouse.IsOver(inRect))
            {
                TooltipHandler.TipRegion(inRect, GetBeardTooltip(beard));
            }
            if (Widgets.ButtonInvisible(inRect))
            {
                SelPawn.style.beardDef = beard;
                SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }

        public TipSignal GetBeardTooltip(BeardDef def)
        {
            TipSignal tip = string.Format("{0}", def.label.CapitalizeFirst() ?? def.defName);
            if (!def.styleTags.NullOrEmpty())
            {
                tip += ("\n" + "PawnAdjust.StyleTags".Translate() + def.styleTags.ToString()).Colorize(Color.grey);
            }
            if(def.modContentPack != null && def.modContentPack.Name != null)
            {
                tip += "\n" + "PawnAdjust.ModSource".Translate() + def.modContentPack.Name.Colorize(Color.blue);
            }
            return tip;
        }

        public IEnumerable<BeardDef> GetAllCompatibleBeardStyles()
        {
            foreach (BeardDef head in DefDatabase<BeardDef>.AllDefs)
            {
                if (CanUseBeardStyle(head) && !head.HasModExtension<DefModExt_HideInAdjuster>())
                {
                    yield return head;
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
            return true;
        }

        #endregion

        #region Selector: Hair Colors

        public void DoSelector_HairColors(Listing_Standard listing)
        {
            if(!ModLister.CheckIdeology("Styling station"))
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
                    delegate(Color color) 
                    { 
                        SelPawn.story.HairColor = color; 
                        SelPawn.drawer.renderer.graphics.ResolveAllGraphics(); 
                    });
                SetSectionHeight(selectorString, outputHeight);
                listing.EndSection(hairColorListing);
            }
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

        #endregion

        #region Selector: Tattoo Style

        public void DoSelector_TattooStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_TattooStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_TattooStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                List<TattooDef> allTattooStyles = GetAllCompatibleTattooStyles().ToList();
                allTattooStyles.SortBy(gd => gd.defName);
                Listing_Standard headTypeListing = listing.BeginSection(GetSectionHeight(selectorString));
                float abCurY = 0f;
                float abCurX = 0f;
                for (int i = 0; i < allTattooStyles.Count(); i++)
                {
                    DrawTattooStyleSelector(new Rect(abCurX, abCurY, 80f, 75f), headTypeListing, allTattooStyles[i]);
                    // Handle Row/Column Position.
                    if (i + 1 < allTattooStyles.Count())
                    {
                        if ((i + 1) % 5 == 0)
                        {
                            abCurY += 80f;
                            abCurX = 0f;
                        }
                        else
                        {
                            abCurX += 80f;
                        }
                    }
                }
                SetSectionHeight(selectorString, abCurY + 80f);
                listing.EndSection(headTypeListing);
            }
        }

        public void DrawTattooStyleSelector(Rect rect, Listing_Standard listing, TattooDef tattoo)
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
            GUI.color = color;
            Widgets.DefIcon(rect, tattoo);
            GUI.color = Color.white;
            if (Mouse.IsOver(inRect))
            {
                TooltipHandler.TipRegion(inRect, GetTattooTooltip(tattoo));
            }
            if (Widgets.ButtonInvisible(inRect))
            {
                SelPawn.style.FaceTattoo = tattoo;
                SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }

        public TipSignal GetTattooTooltip(TattooDef def)
        {
            TipSignal tip = string.Format("{0}", def.label.CapitalizeFirst() ?? def.defName);
            if (!def.styleTags.NullOrEmpty())
            {
                tip += ("\n" + "PawnAdjust.StyleTags".Translate() + def.styleTags.ToString()).Colorize(Color.grey);
            }
            if (def.modContentPack != null && def.modContentPack.Name != null)
            {
                tip += "\n" + "PawnAdjust.ModSource".Translate() + def.modContentPack.Name.Colorize(Color.blue);
            }
            return tip;
        }

        public IEnumerable<TattooDef> GetAllCompatibleTattooStyles()
        {
            foreach (TattooDef head in DefDatabase<TattooDef>.AllDefs)
            {
                if (CanUseTattooStyle(head) && head.tattooType == TattooType.Face && !head.HasModExtension<DefModExt_HideInAdjuster>())
                {
                    yield return head;
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
            return true;
        }

        #endregion

        #region Selector: Body Tattoo Style

        public void DoSelector_BodyTattooStyle(Listing_Standard listing)
        {
            string selectorString = "Selector_BodyTattooStyle";
            bool selectorOpen = GetSectionOpen(selectorString);
            listing.LabelBackedHeader("PawnAdjust.Selector_BodyTattooStyle".Translate(), Color.white, ref selectorOpen, GameFont.Small);
            SetSectionOpen(selectorString, selectorOpen);
            if (!selectorOpen)
            {
                List<TattooDef> allTattooStyles = GetAllCompatibleBodyTattooStyles().ToList();
                allTattooStyles.SortBy(gd => gd.defName);
                Listing_Standard headTypeListing = listing.BeginSection(GetSectionHeight(selectorString));
                float abCurY = 0f;
                float abCurX = 0f;
                for (int i = 0; i < allTattooStyles.Count(); i++)
                {
                    DrawBodyTattooStyleSelector(new Rect(abCurX, abCurY, 80f, 75f), headTypeListing, allTattooStyles[i]);
                    // Handle Row/Column Position.
                    if (i + 1 < allTattooStyles.Count())
                    {
                        if ((i + 1) % 5 == 0)
                        {
                            abCurY += 80f;
                            abCurX = 0f;
                        }
                        else
                        {
                            abCurX += 80f;
                        }
                    }
                }
                SetSectionHeight(selectorString, abCurY + 80f);
                listing.EndSection(headTypeListing);
            }
        }

        public void DrawBodyTattooStyleSelector(Rect rect, Listing_Standard listing, TattooDef tattoo)
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
            GUI.color = color;
            Widgets.DefIcon(rect, tattoo);
            GUI.color = Color.white;
            if (Mouse.IsOver(inRect))
            {
                TooltipHandler.TipRegion(inRect, GetTattooTooltip(tattoo));
            }
            if (Widgets.ButtonInvisible(inRect))
            {
                SelPawn.style.BodyTattoo = tattoo;
                SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }

        public IEnumerable<TattooDef> GetAllCompatibleBodyTattooStyles()
        {
            foreach (TattooDef head in DefDatabase<TattooDef>.AllDefs)
            {
                if (CanUseBodyTattooStyle(head) && head.tattooType == TattooType.Body && !head.HasModExtension<DefModExt_HideInAdjuster>())
                {
                    yield return head;
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
            return true;
        }

        #endregion

        #region Selector: Clothing Colors

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
                foreach(Apparel item in SelPawn.apparel.WornApparel)
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
                            SelPawn.drawer.renderer.graphics.ResolveAllGraphics(); 
                        }, item.def.uiIcon);
                    bool flag = false;
                    if (SelPawn.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        Rect ideoRect = new Rect(apparelItemBox.curX + 30f, apparelItemBox.curY + outputHeight, 150f, 24f);
                        if (Widgets.ButtonText(ideoRect, "SetIdeoColor".Translate()))
                        {
                            item.SetColor(SelPawn.Ideo.ApparelColor);
                            SelPawn.drawer.renderer.graphics.ResolveAllGraphics();
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
                            SelPawn.drawer.renderer.graphics.ResolveAllGraphics();
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


        #endregion

        #region Selector: Royal Title

        public void DoSelector_RoyalTitle(Listing_Standard listing)
        {

        }

        #endregion

        #region Selector: Ideoligion

        public void DoSelector_Ideoligion(Listing_Standard listing)
        {

        }

        #endregion

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
                DoQuickCheat_GivePsylink(sectionListing);
                DoQuickCheat_GiveMechlink(sectionListing);

                SetSectionHeight(selectorString, sectionListing.MaxColumnHeightSeen);
                listing.EndSection(sectionListing);
            }
        }

        public void DoQuickCheat_StripOff(Listing_Standard listing)
        {
            Pawn p = SelPawn;
            if (listing.ButtonText("Strip Naked"))
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
                p.ageTracker.DebugSetAge(21 * 3600000);
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
                SelPawn.Drawer.renderer.graphics.ResolveAllGraphics();
            }
        }

        #endregion
    }
}
