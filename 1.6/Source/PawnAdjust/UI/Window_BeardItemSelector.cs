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
    public class Window_BeardItemSelector : Window_StyleItemSelector
    {
        public Dictionary<string, Texture2D> cachedHairTextures = new Dictionary<string, Texture2D>();

        public QuickSearchWidget quickSearchBeard = new QuickSearchWidget();

        public override string headerLabel => "Beard Selection";

        public Window_BeardItemSelector()
        {

        }

        public override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            if (PawnAdjustMod.settings.beardSelectorSize != null)
            {
                windowRect.width = PawnAdjustMod.settings.beardSelectorSize.x;
                windowRect.height = PawnAdjustMod.settings.beardSelectorSize.y;
            }
            else
            {
                windowRect.width = 900f;
                windowRect.height = 700f;
            }
            if (PawnAdjustMod.settings.beardSelectorPos != null)
            {
                windowRect.x = PawnAdjustMod.settings.beardSelectorPos.x;
                windowRect.y = PawnAdjustMod.settings.beardSelectorPos.y;
            }
            else
            {
                windowRect.x = ((float)UI.screenWidth - 900f) / 2f;
                windowRect.y = ((float)UI.screenHeight - 700f) / 2f;
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            PawnAdjustMod.settings.beardSelectorSize.x = windowRect.width;
            PawnAdjustMod.settings.beardSelectorSize.y = windowRect.height;
            PawnAdjustMod.settings.beardSelectorPos.x = windowRect.x;
            PawnAdjustMod.settings.beardSelectorPos.y = windowRect.y;
            PawnAdjustMod.settings.Write();
            base.Close(doCloseSound);
        }

        public override void DoContents(Listing_Standard listing)
        {
            Rect rect = listing.GetRect(30f);
            quickSearchBeard.OnGUI(rect);
            List<BeardDef> allBeardStyles = GetAllCompatibleBeardStyles(quickSearchBeard.filter.Text).ToList();
            allBeardStyles.SortBy(gd => gd.modContentPack.loadOrder, gd => gd.label ?? gd.defName);
            allBeardStyles.Move(BeardDefOf.NoBeard, 0);
            SelectorArray(listing, allBeardStyles);
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
            if(SelPawn == null) { Close(); return false; }
            Pawn pawn = SelPawn;
            if (ModsConfig.BiotechActive && beard.requiredGene != null)
            {
                if (pawn.genes == null)
                {
                    return false;
                }
                if (!pawn.genes.HasActiveGene(beard.requiredGene))
                {
                    return false;
                }
            }
            return pawn.genes?.StyleItemAllowed(beard) ?? true;
        }
    }
}
