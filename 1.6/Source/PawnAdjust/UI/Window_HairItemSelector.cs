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
    public class Window_HairItemSelector : Window_StyleItemSelector
    {
        public Dictionary<string, Texture2D> cachedHairTextures = new Dictionary<string, Texture2D>();

        public QuickSearchWidget quickSearchHair = new QuickSearchWidget();

        public override string headerLabel => "Hair Selection";

        public Window_HairItemSelector()
        {

        }

        public override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            if(PawnAdjustMod.settings.hairSelectorSize != null)
            {
                windowRect.width = PawnAdjustMod.settings.hairSelectorSize.x;
                windowRect.height = PawnAdjustMod.settings.hairSelectorSize.y;
            }
            else
            {
                windowRect.width = 900f;
                windowRect.height = 700f;
            }
            if(PawnAdjustMod.settings.hairSelectorPos != null)
            {
                windowRect.x = PawnAdjustMod.settings.hairSelectorPos.x;
                windowRect.y = PawnAdjustMod.settings.hairSelectorPos.y;
            }
            else
            {
                windowRect.x = ((float)UI.screenWidth - 900f) / 2f;
                windowRect.y = ((float)UI.screenHeight - 700f) / 2f;
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            PawnAdjustMod.settings.hairSelectorSize.x = windowRect.width;
            PawnAdjustMod.settings.hairSelectorSize.y = windowRect.height;
            PawnAdjustMod.settings.hairSelectorPos.x = windowRect.x;
            PawnAdjustMod.settings.hairSelectorPos.y = windowRect.y;
            PawnAdjustMod.settings.Write();
            base.Close(doCloseSound);
        }

        public override void DoContents(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);
            quickSearchHair.OnGUI(rect);
            List<HairDef> allHairStyles = GetAllCompatibleHairStyles(quickSearchHair.filter.Text).ToList();
            allHairStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
            allHairStyles.Move(HairDefOf.Bald, 0);
            SelectorArray(listing, allHairStyles);
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
            if (SelPawn == null) { Close(); return false; }
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && hair.requiredGene != null)
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                if (!pawn.genes.HasActiveGene(hair.requiredGene))
                {
                    return false;
                }
            }
            return pawn.genes?.StyleItemAllowed(hair) ?? true;
        }
    }
}
