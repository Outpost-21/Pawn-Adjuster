using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace PawnAdjust
{
    public class PawnAdjustMod : Mod
    {
        public static PawnAdjustMod mod;
        public static PawnAdjustSettings settings;

        public Vector2 optionsScrollPosition;
        public float optionsViewRectHeight;

        internal static string VersionDir => Path.Combine(mod.Content.ModMetaData.RootDir.FullName, "Version.txt");
        public static string CurrentVersion { get; private set; }

        public PawnAdjustMod(ModContentPack content) : base(content)
        {
            mod = this;
            settings = GetSettings<PawnAdjustSettings>();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            CurrentVersion = $"{version.Major}.{version.Minor}.{version.Build}";

            LogUtil.LogMessage($"{CurrentVersion} ::");

            if (Prefs.DevMode)
            {
                File.WriteAllText(VersionDir, CurrentVersion);
            }

            Harmony harmony = new Harmony("Neronix17.PawnAdjust.RimWorld");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public override string SettingsCategory() => "Pawn Adjuster";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            bool flag = optionsViewRectHeight > inRect.height;
            Rect viewRect = new Rect(inRect.x, inRect.y, inRect.width - (flag ? 26f : 0f), optionsViewRectHeight);
            Widgets.BeginScrollView(inRect, ref optionsScrollPosition, viewRect);
            Listing_Standard listing = new Listing_Standard();
            Rect rect = new Rect(viewRect.x, viewRect.y, viewRect.width, 999999f);
            listing.Begin(rect);
            // ============================ CONTENTS ================================
            DoOptionsCategoryContents(listing);
            // ======================================================================
            optionsViewRectHeight = listing.CurHeight;
            listing.End();
            Widgets.EndScrollView();
        }

        public void DoOptionsCategoryContents(Listing_Standard listing)
        {
            listing.CheckboxEnhanced("PawnAdjust.Setting_LegacySubsections".Translate(), "PawnAdjust.Setting_LegacySubsectionsDesc".Translate(), ref settings.legacySubsections);
            if (listing.ButtonTextLabeled("PawnAdjust.Setting_ResetStyleWindows".Translate(), "PawnAdjust.Setting_ResetButton".Translate(), tooltip: "PawnAdjust.Setting_ResetStyleWindowsDesc".Translate()))
            {
                settings.hairSelectorSize = new Vector2(900f, 700f);
                settings.beardSelectorSize = new Vector2(900f, 700f);
                settings.headTattooSelectorSize = new Vector2(900f, 700f);
                settings.bodyTattooSelectorSize = new Vector2(900f, 700f);

                settings.hairSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);
                settings.beardSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);
                settings.headTattooSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);
                settings.bodyTattooSelectorPos = new Vector2(((float)UI.screenWidth - 900f) / 2f, ((float)UI.screenHeight - 700f) / 2f);
            }
        }
    }
}
