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
    public class PawnAdjustSettings : ModSettings
    {
        public bool verboseLogging = false;

        public bool legacySubsections = false;

        public Vector2 hairSelectorSize = new Vector2(900f, 700f);
        public Vector2 beardSelectorSize = new Vector2(900f, 700f);
        public Vector2 headTattooSelectorSize = new Vector2(900f, 700f);
        public Vector2 bodyTattooSelectorSize = new Vector2(900f, 700f);

        public Vector2 hairSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);
        public Vector2 beardSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);
        public Vector2 headTattooSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);
        public Vector2 bodyTattooSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref legacySubsections, "legacySubsections", false, true);

            Scribe_Values.Look(ref hairSelectorSize, "hairSelectorSize");
            Scribe_Values.Look(ref beardSelectorSize, "beardSelectorSize");
            Scribe_Values.Look(ref headTattooSelectorSize, "headTattooSelectorSize");
            Scribe_Values.Look(ref bodyTattooSelectorSize, "bodyTattooSelectorSize");

            Scribe_Values.Look(ref hairSelectorPos, "hairSelectorPos");
            Scribe_Values.Look(ref beardSelectorPos, "beardSelectorPos");
            Scribe_Values.Look(ref headTattooSelectorPos, "headTattooSelectorPos");
            Scribe_Values.Look(ref bodyTattooSelectorPos, "bodyTattooSelectorPos");
        }

        public bool IsValidSetting(string input)
        {
            if (GetType().GetFields().Where(p => p.FieldType == typeof(bool)).Any(i => i.Name == input))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<string> GetEnabledSettings
        {
            get
            {
                return GetType().GetFields().Where(p => p.FieldType == typeof(bool) && (bool)p.GetValue(this)).Select(p => p.Name);
            }
        }
    }
}
