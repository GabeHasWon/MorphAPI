using MorphAPI.Core.Morphing;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

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
    /// <remarks>
    /// This is the player-instance-local copy of the morph, driven by network packets. Mutate it through the
    /// <see cref="Extensions"/> methods rather than assigning it directly, so clients and the server stay in sync.
    /// </remarks>
    public Morph? ActiveMorph { get; internal set; } = null;

    /// <summary>
    /// Subscribes the morph drawing override.
    /// </summary>
    /// <remarks>
    /// Called once by tModLoader when the mod loads. Do not call this yourself.
    /// </remarks>
    public override void Load() => On_PlayerDrawLayers.DrawPlayer_RenderAllLayers += DrawMorphIfAny;

    /// <summary>
    /// Removes the morph drawing override.
    /// </summary>
    /// <remarks>
    /// Called once by tModLoader when the mod unloads. Do not call this yourself.
    /// </remarks>
    public override void Unload() => On_PlayerDrawLayers.DrawPlayer_RenderAllLayers -= DrawMorphIfAny;

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
    /// <remarks>
    /// Runs on every side, including a dedicated server, so morph updates must not assume they are on a client.
    /// </remarks>
    public override void UpdateEquips() => ActiveMorph?.Update(Player);

    /// <summary>
    /// Lets the morph modify the player's draw info.
    /// </summary>
    /// <param name="drawInfo">The player's draw info.</param>
    /// <remarks>
    /// Only runs on clients, as part of the drawing flow.
    /// </remarks>
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (Player.TryGetMorph(out Morph? morph))
            MorphLoader.ModifyDrawInfo(morph, ref drawInfo);
    }

    /// <summary>
    /// Lets the morph block item usage.
    /// </summary>
    /// <param name="item">The item being used.</param>
    /// <returns>True if the item may be used.</returns>
    /// <remarks>
    /// Runs as the local client item use gate; the morph's <see cref="Morph.CanUseItem(Player, Item)"/> return value decides.
    /// </remarks>
    public override bool CanUseItem(Item item)
    {
        if (Player.TryGetMorph(out Morph? morph))
            return morph.CanUseItem(Player, item);

        return true;
    }

    /// <summary>
    /// Sends the player's existing morph to clients when the player finishes joining, so late joiners stay in sync.
    /// </summary>
    /// <param name="toWho">The client index(es) to send the morph to.</param>
    /// <param name="fromWho">The client that triggered the sync.</param>
    /// <param name="newPlayer">Whether the player is newly joined.</param>
    /// <remarks>
    /// tModLoader invokes this hook on both the joining local client and the server, but this implementation only
    /// sends from the server; the client invocation returns without sending. <paramref name="toWho"/> and
    /// <paramref name="fromWho"/> are passed through to <see cref="MorphAPI.SendSetMorph(Morph, Player, int, int)"/> as-is.
    /// </remarks>
    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        if (Main.netMode != NetmodeID.Server || !Player.TryGetMorph(out Morph? morph))
            return;

        MorphAPI.SendSetMorph(morph, Player, toWho, fromWho);
    }
}
