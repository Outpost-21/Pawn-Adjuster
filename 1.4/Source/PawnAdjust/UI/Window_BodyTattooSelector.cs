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
    public class Window_BodyTattooSelector : Window_StyleItemSelector
    {
        public Dictionary<string, Texture2D> cachedHairTextures = new Dictionary<string, Texture2D>();

        public QuickSearchWidget quickSearchBodyTattoo = new QuickSearchWidget();

        public override string headerLabel => "Body Tattoo Selection";

        public Window_BodyTattooSelector()
        {

        }

        public override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            if (PawnAdjustMod.settings.bodyTattooSelectorSize != null)
            {
                windowRect.width = PawnAdjustMod.settings.bodyTattooSelectorSize.x;
                windowRect.height = PawnAdjustMod.settings.bodyTattooSelectorSize.y;
            }
            else
            {
                windowRect.width = 900f;
                windowRect.height = 700f;
            }
            if (PawnAdjustMod.settings.bodyTattooSelectorPos != null)
            {
                windowRect.x = PawnAdjustMod.settings.bodyTattooSelectorPos.x;
                windowRect.y = PawnAdjustMod.settings.bodyTattooSelectorPos.y;
            }
            else
            {
                windowRect.x = ((float)UI.screenWidth - 900f) / 2f;
                windowRect.y = ((float)UI.screenHeight - 700f) / 2f;
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            PawnAdjustMod.settings.bodyTattooSelectorSize.x = windowRect.width;
            PawnAdjustMod.settings.bodyTattooSelectorSize.y = windowRect.height;
            PawnAdjustMod.settings.bodyTattooSelectorPos.x = windowRect.x;
            PawnAdjustMod.settings.bodyTattooSelectorPos.y = windowRect.y;
            PawnAdjustMod.settings.Write();
            base.Close(doCloseSound);
        }

        public override void DoContents(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);
            quickSearchBodyTattoo.OnGUI(rect);
            List<TattooDef> allTattooStyles = GetAllCompatibleBodyTattooStyles(quickSearchBodyTattoo.filter.Text).ToList();
            allTattooStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
            allTattooStyles.Move(TattooDefOf.NoTattoo_Body, 0);
            SelectorArray(listing, allTattooStyles);
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
            if (SelPawn == null) { Close(); return false; }
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
    }
}
