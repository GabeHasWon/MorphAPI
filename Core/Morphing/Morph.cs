using System;
using System.Collections.Generic;
using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

/// <summary>
/// Defines a morph, which includes drawing, movement, hitbox and usage.
/// </summary>
public abstract class Morph : ModType
{
    /// <summary>
    /// Whether this morph hides the default draw layers entirely. Defaults to false.
    /// </summary>
    public virtual bool HideDefaultPlayer => false;

    /// <summary>
    /// Registers this morph to the lookup.
    /// </summary>
    protected sealed override void Register() => ModTypeLookup<Morph>.Register(this);

    /// <summary>
    /// Modifies the hitbox if desired. The resulting hitbox will be of the resulting <paramref name="size"/> if this method returns true.<br/>
    /// This will do NOTHING if it returns false!
    /// </summary>
    /// <param name="player">The player who's hitbox is being modified.</param>
    /// <param name="size">Size of the new hitbox. Defaults to the player's default hitbox size.</param>
    /// <returns>Whether to run the hitbox modification code or not.</returns>
    public virtual bool ModifyHitbox(Player player, out Point16 size)
    {
        size = new Point16(Player.defaultWidth, Player.defaultHeight);
        return false;
    }

    /// <summary>
    /// Called when this morph is activated.
    /// </summary>
    public virtual void OnMorph(Player player)
    {
    }

    /// <summary>
    /// Called when this morph is deactivated.
    /// </summary>
    public virtual void OnUnmorph(Player player)
    {
    }

    /// <summary>
    /// Called every frame when this morph is active.
    /// </summary>
    /// <param name="player"></param>
    public virtual void Update(Player player)
    {
    }

    /// <inheritdoc cref="ModPlayer.ModifyDrawInfo(ref PlayerDrawSet)"/>
    public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
    }

    /// <summary>
    /// Runs before the draw data cache is cleared, allowing morphs to retain certain draw data. Returns an empty list by default.
    /// </summary>
    /// <param name="drawInfo">The player's draw info.</param>
    public virtual List<DrawData> PreClearDrawCache(ref PlayerDrawSet drawInfo) => [];

    /// <summary>
    /// Allows modification of draw layers, including the draw layers preserved from <see cref="PreClearDrawCache(ref PlayerDrawSet)"/>.
    /// </summary>
    /// <param name="oldDrawData">Preserved layers from <see cref="PreClearDrawCache(ref PlayerDrawSet)"/>.</param>
    /// <param name="drawInfo">The player's draw info.</param>
    public virtual void SetDrawLayers(List<DrawData> oldDrawData, ref PlayerDrawSet drawInfo)
    {
    }
}
