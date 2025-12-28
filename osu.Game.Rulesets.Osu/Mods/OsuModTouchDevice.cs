// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// osu! specific Touch device mod. Attaches pinch-to-zoom handling to the drawable ruleset's playfield when applied.
    /// </summary>
    public class OsuModTouchDevice : ModTouchDevice, IApplicableToDrawableRuleset<OsuHitObject>
    {
        public override Type[] IncompatibleMods =>
            base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModBloom) }).ToArray();

        // Mod sempre válido para submissão em dispositivos móveis (como no osu!droid)
        public override bool Ranked => true;

        /// <summary>
        /// When this mod is applied to a drawable ruleset, attach the pinch-to-zoom handler to the playfield adjustment container.
        /// This ensures touch devices automatically get pinch-to-zoom while preserving gameplay logic.
        /// </summary>
        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            try
            {
                var container = drawableRuleset.PlayfieldAdjustmentContainer;
                if (container != null)
                {
                    // Add handler if not already present (defensive).
                    if (container.FindChildOfType<PinchZoomHandler>() == null)
                        container.Add(new PinchZoomHandler { RelativeSizeAxes = osu.Framework.Graphics.Axes.Both });
                }
            }
            catch (Exception)
            {
                // Fail-safe: do not crash the game if adding fails for any reason.
            }
        }
    }

    // Small helper extension used above to check existing handler presence.
    internal static class DrawableContainerExtensions
    {
        public static T? FindChildOfType<T>(this Container c) where T : Drawable
        {
            foreach (var child in c.Children)
            {
                if (child is T t)
                    return t;
            }

            return null;
        }
    }
}
