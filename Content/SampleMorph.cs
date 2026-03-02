using System;
using System.Collections.Generic;
using MorphAPI.Core.Morphing;
using Terraria.DataStructures;
using Terraria.ID;

namespace MorphAPI.Content;

/// <summary>
/// Defines a sample morph, which looks like a rolling dirt block.<br/>
/// Make sure to remove the Autoload attribute (or set EnableMorph to true) if you want to use this!
/// </summary>
[Autoload(EnableMorph)]
public class SampleMorph : Morph
{
    /// <summary>
    /// Whether to enable this morph for testing.
    /// </summary>
    public const bool EnableMorph = true;

    /// <inheritdoc/>
    public override bool HideDefaultPlayer => true;

    internal float rotation = 0;

    /// <summary>
    /// Allows the block to stay stationary in mounts, roll when moving, and to stop rolling if moving slowly enough.
    /// </summary>
    /// <param name="player"></param>
    public override void Update(Player player)
    {
        if (!player.mount.Active)
        {
            if (Math.Abs(player.velocity.X) > 0.2f)
                rotation += player.velocity.X / 12f;
            else
                rotation = 0;
        }
        else
            rotation = 0;
    }

    /// <summary>
    /// Forces the hitbox to 16 pixels tall and wide. Returns true as the hitbox has been modified.
    /// </summary>
    public override bool ModifyHitbox(Player player, out Point16 size)
    {
        size = new Point16(16, 16);
        return true;
    }

    /// <summary>
    /// Spawns vfx.
    /// </summary>

    public override void OnMorph(Player player)
    {
        for (int i = 0; i < 16; i++)
            Dust.NewDust(player.position, player.width, player.height, DustID.Dirt);
    }

    /// <summary>
    /// Spawns vfx.
    /// </summary>
    public override void OnUnmorph(Player player)
    {
        for (int i = 0; i < 16; i++)
            Dust.NewDust(player.position, player.width, player.height, DustID.Dirt);
    }

    /// <summary>
    /// As this replaces the default layers, this method is used to draw the mount's back, the morph's layer, and the mount's front.
    /// </summary>
    public override void SetDrawLayers(List<DrawData> oldDrawData, ref PlayerDrawSet drawInfo)
    {
        PlayerDrawLayers.MountBack.DrawWithTransformationAndChildren(ref drawInfo);
        ModContent.GetInstance<SampleMorphDrawLayer>().DrawWithTransformationAndChildren(ref drawInfo);
        PlayerDrawLayers.MountFront.DrawWithTransformationAndChildren(ref drawInfo);
    }
}
