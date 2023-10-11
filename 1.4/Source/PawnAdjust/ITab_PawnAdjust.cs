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
    public class ITab_PawnAdjust : ITab
    {
        public static readonly Vector2 WinSize = new Vector2(480f, 510f);

        public Vector2 optionsScrollPosition;
        public float optionsViewRectHeight;

        public Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

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

		public override bool IsVisible => base.IsVisible && CanControlPawn;

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
                    if ((i + 1) % 5 == 0)
                    {
                        abCurY += 80f;
                        abCurX = 0f;
                    }
                    else if (i == allHeadTypes.Count())
                    {
                        abCurX += 80f;
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
                TipSignal tip = string.Format("{0}", head.label.CapitalizeFirst() ?? string.Empty);
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
            Widgets.DrawTextureFitted(rect, GetHeadTexture(def), 1f * 0.85f, Vector2.one, new Rect(0f, 0f, 1f, 1f), 0, buttonMat);
            rect.position += new Vector2(rect.size.x, 0f);
            GUI.color = Color.white;
        }

        public Texture2D GetHeadTexture(HeadTypeDef def)
        {
            if (!cachedTextures.ContainsKey(def.defName))
            {
                cachedTextures.Add(def.defName, (Texture2D)def.GetGraphic(SelPawn.story.SkinColor).MatSouth.mainTexture ?? BaseContent.BadTex);
            }
            return cachedTextures[def.defName];
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
                List<BodyTypeDef> allHeadTypes = GetAllCompatibleBodyTypes().ToList();
                allHeadTypes.SortBy(gd => gd.defName);
                Listing_Standard headTypeListing = listing.BeginSection(GetSectionHeight(selectorString));
                float abCurY = 0f;
                float abCurX = 0f;
                for (int i = 0; i < allHeadTypes.Count(); i++)
                {
                    DrawBodyTypeSelector(new Rect(abCurX, abCurY, 80f, 75f), headTypeListing, allHeadTypes[i]);
                    // Handle Row/Column Position.
                    if ((i + 1) % 5 == 0)
                    {
                        abCurY += 80f;
                        abCurX = 0f;
                    }
                    else if (i == allHeadTypes.Count())
                    {
                        abCurX += 80f;
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
                TipSignal tip = string.Format("{0}", body.label.CapitalizeFirst() ?? string.Empty);
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
            Widgets.DrawTextureFitted(rect, GetBodyTexture(def), 1f * 0.85f, Vector2.one, new Rect(0f, 0f, 1f, 1f), 0, buttonMat);
            rect.position += new Vector2(rect.size.x, 0f);
            GUI.color = Color.white;
        }

        public Texture2D GetBodyTexture(BodyTypeDef def)
        {
            if (!cachedTextures.ContainsKey(def.defName))
            {
                cachedTextures.Add(def.defName, (Texture2D)GraphicDatabase.Get<Graphic_Multi>(def.bodyNakedGraphicPath, ShaderDatabase.Cutout, new Vector2(1f, 1f), SelPawn.story.SkinColor).MatSouth.mainTexture ?? BaseContent.BadTex);
            }
            return cachedTextures[def.defName];
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

        }

        #endregion

        #region Selector: Beard Style

        public void DoSelector_BeardStyle(Listing_Standard listing)
        {

        }

        #endregion

        #region Selector: Tattoo Style

        public void DoSelector_TattooStyle(Listing_Standard listing)
        {

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
            return;
            if (!Prefs.DevMode)
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
                sectionListing.NewColumn();
                DoQuickCheat_GivePsylink(sectionListing);
                DoQuickCheat_GiveMechlink(sectionListing);


                SetSectionHeight(selectorString, sectionListing.MaxColumnHeightSeen);
                listing.EndSection(sectionListing);
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

        #endregion
    }
}
