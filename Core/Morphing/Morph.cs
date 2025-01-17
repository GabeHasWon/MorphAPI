using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

internal abstract class Morph
{
    /// <summary>
    /// Modifies the hitbox if desired. The resulting hitbox will be of the resulting <paramref name="size"/>.
    /// </summary>
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
}
