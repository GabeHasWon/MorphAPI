using MorphAPI.Core.Morphing;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace MorphAPI.Core;

#nullable enable

/// <summary>
/// Handles morph functionality, and stores the player's current morph (if any).
/// </summary>
public class MorphPlayer : ModPlayer
{
    /// <summary>
    /// The player's current morph, if any.
    /// </summary>
    public Morph? ActiveMorph { get; internal set; } = null;

    /// <summary>
    /// Adds the override-draw hook.
    /// </summary>
    public override void Load() => On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += DrawMorphIfAny;

    private void DrawMorphIfAny(On_PlayerDrawLayers.orig_DrawPlayer_RenderAllLayers orig, ref PlayerDrawSet drawinfo)
    {
        if (!drawinfo.drawPlayer.TryGetMorph(out Morph? morph) || !morph.HideDefaultPlayer)
            orig(ref drawinfo);
        else //Draws only the slime when active
        {
            List<DrawData> oldDrawData = morph.PreClearDrawCache(ref drawinfo);
            drawinfo.DrawDataCache.Clear();

            morph.SetDrawLayers(oldDrawData, ref drawinfo);
            orig(ref drawinfo);
        }
    }

    /// <summary>
    /// Updates the morph, if any.
    /// </summary>
    public override void UpdateEquips() => ActiveMorph?.Update(Player);

    /// <summary>
    /// Modifies the drawing of the morph, if any.
    /// </summary>
    /// <param name="drawInfo"></param>
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (Player.HasMorph())
            MorphLoader.ModifyDrawInfo(ActiveMorph, ref drawInfo);
    }

    /// <summary>
    /// Hook used for <see cref="Morph.CanUseItem(Player, Item)"/> functionality.
    /// </summary>
    public override bool CanUseItem(Item item)
    {
        if (Player.TryGetMorph(out Morph? morph))
            return morph.CanUseItem(Player, item);

        return true;
    }
}
