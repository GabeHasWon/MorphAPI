using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

/// <summary>
/// Defines a global hook that applies to every active morph (and every player using one).<br/>
/// Subclass it in your mod to react to all morphs at once; subclasses are autoloaded automatically.
/// </summary>
/// <remarks>
/// Example usage:
/// <code>
/// public class MyGlobalMorph : GlobalMorph
/// {
///     public override void OnMorph(Morph morph, Player player)
///     {
///     }
/// }
/// </code>
/// </remarks>
public class GlobalMorph : ModType
{
    /// <summary>
    /// Registers this global morph with the loader and subscribes its hooks. Called automatically by the loader; do not call this yourself.
    /// </summary>
    protected override void Register()
    {
        ModTypeLookup<GlobalMorph>.Register(this);
        MorphLoader.RegisterGlobalMorph(this);
    }

    /// <summary>
    /// Modifies the hitbox of any active morph, after the morph's own <see cref="Morph.ModifyHitbox(Player, out Point16)"/> has approved a size.
    /// </summary>
    /// <param name="morph">The morph being used.</param>
    /// <param name="player">The player using the morph.</param>
    /// <param name="size">The size of the hitbox; modify it to adjust the result.</param>
    /// <returns>True (the default) to allow the hitbox, false to veto it and keep the vanilla hitbox. The first global to return false short circuits the rest.</returns>
    /// <remarks>
    /// This only runs when the morph itself returned true from its own <see cref="Morph.ModifyHitbox(Player, out Point16)"/> and the player is not mounted.
    /// </remarks>
    public virtual bool ModifyHitbox(Morph morph, Player player, ref Point16 size) => true;

    /// <summary>
    /// Called when any morph is activated.
    /// </summary>
    /// <param name="morph">The morph being activated.</param>
    /// <param name="player">The player using the morph.</param>
    /// <remarks>
    /// Runs on every side that applies the morph (server and clients), so guard client-only effects.
    /// </remarks>
    public virtual void OnMorph(Morph morph, Player player)
    {
    }

    /// <summary>
    /// Called when any morph is deactivated.
    /// </summary>
    /// <param name="morph">The morph being deactivated.</param>
    /// <param name="player">The player using the morph.</param>
    /// <remarks>
    /// Runs on every side that applies the morph (server and clients), so guard client-only effects.
    /// </remarks>
    public virtual void OnUnmorph(Morph morph, Player player)
    {
    }

    /// <summary>
    /// Allows modifying the draw info of any morphed player.
    /// </summary>
    /// <param name="morph">The morph being drawn.</param>
    /// <param name="drawInfo">The player's draw info.</param>
    /// <remarks>
    /// Only runs on clients, as part of the drawing flow.
    /// </remarks>
    public virtual void ModifyDrawInfo(Morph morph, ref PlayerDrawSet drawInfo)
    {
    }
}
