// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
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
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAutopilot), typeof(OsuModBloom) }).ToArray();

        // Keep ranked behaviour as before (default behaviour).
        public override bool Ranked => UsesDefaultConfiguration;

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
```


O que você deve testar localmente
- Build do projeto (Visual Studio / dotnet).
- Rodar testes que já existem (há testes de OsuModTouchDevice no repositório).
- Teste manual em dispositivo móvel / emulador:
  - Entrar em uma jogada com touch mode (ou garantir que Static.TouchInputActive = true para ativar mod),
  - Tentar pinchar para ampliar/reduzir; verificar se o ponto sob os dedos permanece fixo,
  - Verificar que toques simples (tap) continuam a funcionar quando não ocorrer pinch,
  - Verificar que múltiplos toques não geram hits acidentais durante pinch.

Possíveis ajustes que recomendo depois do teste
- Ajustar min/max scale aos valores desejados (dependendo de layout / HUD).
- Suavização (lerp) ao aplicar target scale para ficar mais agradável.
- Adicionar double-tap-to-reset ou um gesto para resetar o zoom.
- Garantir que HUD / overlays não sejam escalados (caso você queira que HUD permaneça fixo). Atualmente aplicamos escala no PlayfieldAdjustmentContainer que normalmente afeta somente playfield.

Quer que eu:
- 1) gere um patch/PR com esses arquivos (crio a branch e o PR); ou
- 2) apenas te passe o diff/patch para você aplicar manualmente?

Se preferir, eu já crio o branch + PR — me autorize para criar o PR no renakome/osu (ou diga que gere apenas o patch que você aplicará).