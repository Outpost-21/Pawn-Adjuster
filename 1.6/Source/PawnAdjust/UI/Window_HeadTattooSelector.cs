using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnAdjust
{
    public class Window_HeadTattooSelector : Window_StyleItemSelector
    {
        public Dictionary<string, Texture2D> cachedHairTextures = new Dictionary<string, Texture2D>();

        public QuickSearchWidget quickSearchHeadTattoo = new QuickSearchWidget();

        public override string headerLabel => "Head Tattoo Selection";

        public Window_HeadTattooSelector()
        {

        }

        public override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            if (PawnAdjustMod.settings.headTattooSelectorSize != null)
            {
                windowRect.width = PawnAdjustMod.settings.headTattooSelectorSize.x;
                windowRect.height = PawnAdjustMod.settings.headTattooSelectorSize.y;
            }
            else
            {
                windowRect.width = 900f;
                windowRect.height = 700f;
            }
            if (PawnAdjustMod.settings.headTattooSelectorPos != null)
            {
                windowRect.x = PawnAdjustMod.settings.headTattooSelectorPos.x;
                windowRect.y = PawnAdjustMod.settings.headTattooSelectorPos.y;
            }
            else
            {
                windowRect.x = ((float)UI.screenWidth - 900f) / 2f;
                windowRect.y = ((float)UI.screenHeight - 700f) / 2f;
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            PawnAdjustMod.settings.headTattooSelectorSize.x = windowRect.width;
            PawnAdjustMod.settings.headTattooSelectorSize.y = windowRect.height;
            PawnAdjustMod.settings.headTattooSelectorPos.x = windowRect.x;
            PawnAdjustMod.settings.headTattooSelectorPos.y = windowRect.y;
            PawnAdjustMod.settings.Write();
            base.Close(doCloseSound);
        }

        public override void DoContents(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);
            quickSearchHeadTattoo.OnGUI(rect);
            List<TattooDef> allTattooStyles = GetAllCompatibleTattooStyles(quickSearchHeadTattoo.filter.Text).ToList();
            allTattooStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
            allTattooStyles.Move(TattooDefOf.NoTattoo_Face, 0);
            SelectorArray(listing, allTattooStyles);
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
            if (SelPawn == null) { Close(); return false; }
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && tattoo.requiredGene != null)
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                if (!pawn.genes.HasActiveGene(tattoo.requiredGene))
                {
                    return false;
                }
            }
            return pawn.genes?.StyleItemAllowed(tattoo) ?? true;
        }
    }
}
