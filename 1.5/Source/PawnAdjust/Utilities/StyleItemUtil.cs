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
    public static class StyleItemUtil
    {
        public static string GetStyleTooltip(this StyleItemDef def)
        {
            string tip = string.Format("{0}", def.label.CapitalizeFirst() ?? def.defName);
            if (!def.styleTags.NullOrEmpty())
            {
                tip += "\n" + "PawnAdjust.StyleTags".Translate();
                foreach (string s in def.styleTags)
                {
                    tip += "\n  - " + s.ToString().Colorize(Color.grey);
                }
            }
            if (def.modContentPack != null && def.modContentPack.Name != null)
            {
                tip += "\n" + "PawnAdjust.ModSource".Translate() + def.modContentPack.Name.ToString().Colorize(Color.blue);
            }
            return tip;
        }

        public static bool FitsFilter<T>(string filter, T inDef)
        {
            Def def = inDef as Def;
            if (filter.NullOrEmpty())
            {
                return true;
            }
            if (!def.defName.NullOrEmpty() && def.defName.ToLower().Contains(filter))
            {
                return true;
            }
            if (!def.label.NullOrEmpty() && def.label.ToLower().Contains(filter))
            {
                return true;
            }
            if (!def.description.NullOrEmpty() && def.description.ToLower().Contains(filter))
            {
                return true;
            }
            if (def.modContentPack != null && def.modContentPack.Name.ToLower().Contains(filter))
            {
                return true;
            }
            StyleItemDef styleItemDef = def as StyleItemDef;
            if (styleItemDef != null)
            {
                if (!styleItemDef.styleTags.NullOrEmpty() && styleItemDef.styleTags.Any(st => st.ToLower().Contains(filter)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
