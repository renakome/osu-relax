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
using osu.Game.Beatmaps;
using osuTK; // Vector2

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// osu! specific Touch device mod. Attaches pinch-to-zoom handling to the drawable ruleset's playfield when applied.
    /// This variant adjusts the OsuPlayfield.Scale so hit circle visuals match the osu!droid radius calculation as closely as possible.
    /// </summary>
    public class OsuModTouchDevice : ModTouchDevice, IApplicableToDrawableRuleset<OsuHitObject>, IApplicableToDifficulty
    {
        public override Type[] IncompatibleMods =>
            base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModBloom) }).ToArray();

        // Mod sempre válido para submissão em dispositivos móveis (como no osu!droid)
        public override bool Ranked => true;

        /// <summary>
        /// Não alteramos diretamente a dificuldade aqui — alterações brutais em BeatmapDifficulty podem ser lidas
        /// em locais diferentes do pipeline e causar inconsistências. Mantemos vazio para evitar efeitos colaterais.
        /// </summary>
        public void ApplyToDifficulty(BeatmapDifficulty difficulty)
        {
            // Intencionalmente vazio: a alteração de escala é feita no playfield para replicar o osudroid.
        }

        /// <summary>
        /// Aplica um multiplicador à escala do OsuPlayfield de modo que o raio visual dos hitcircles
        /// coincida com a fórmula "osu!pixels" usada por implementações tipo osudroid.
        /// </summary>
        public void ApplyToDrawableRuleset(DrawableRuleset<OsuHitObject> drawableRuleset)
        {
            try
            {
                var container = drawableRuleset.PlayfieldAdjustmentContainer;
                if (container == null) return;

                // adiciona pinch-zoom handler (se desejar pinch-to-zoom)
                if (container.FindChildOfType<PinchZoomHandler>() == null)
                    container.Add(new PinchZoomHandler { RelativeSizeAxes = osu.Framework.Graphics.Axes.Both });

                // tenta encontrar o OsuPlayfield dentro do container
                var osuPlayfield = container.Children.OfType<OsuPlayfield>().FirstOrDefault();
                if (osuPlayfield == null) return;

                // pega o CS atual do beatmap (guard clauses)
                var beatmap = drawableRuleset.Beatmap;
                if (beatmap == null || beatmap.BeatmapInfo?.Difficulty == null) return;

                float cs = beatmap.BeatmapInfo.Difficulty.CircleSize;

                // 1) raio "osudroid" em osu!pixels (fórmula conhecida)
                double osuDroidRadius = (54.4 - 4.48 * cs) * 1.00041;

                // 2) raio que o lazer gera por padrão (OBJECT_RADIUS = 64)
                double difficultyRange = (cs - 5.0) / 5.0;
                double lazerScale = ((1.0 - 0.7 * difficultyRange) / 2.0) * 1.00041; // aplica fudge para parity
                double lazerRadius = OsuHitObject.OBJECT_RADIUS * lazerScale; // OsuHitObject.OBJECT_RADIUS == 64

                // segurança: evita divisão por zero
                if (lazerRadius <= double.Epsilon) return;

                // 3) multiplicador necessário para igualar raios
                double multiplier = osuDroidRadius / lazerRadius;

                // limita multiplicador para evitar mudanças extremas/indesejadas
                float multiplierClamped = (float)Math.Clamp(multiplier, 0.5, 3.0);

#if DEBUG
                // logs úteis para debug (apague em produção se quiser)
                System.Diagnostics.Debug.WriteLine($"[OsuModTouchDevice] cs={cs:F2} osuDroidRadius={osuDroidRadius:F3} lazerRadius={lazerRadius:F3} rawMult={multiplier:F3} clampedMult={multiplierClamped:F3}");
#endif

                // aplica a escala multiplicativa no playfield (preserva a escala atual)
                osuPlayfield.Scale = osuPlayfield.Scale * multiplierClamped;
            }
            catch (Exception)
            {
                // fail-safe: não deixa o jogo crashar por causa do mod
            }
        }
    }

    // Helper extension usado acima para checar e encontrar handlers já adicionados.
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
