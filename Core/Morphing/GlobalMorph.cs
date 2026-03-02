using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

internal class GlobalMorph : ModType
{
    protected override void Register()
    {
        ModTypeLookup<GlobalMorph>.Register(this);
        MorphLoader.RegisterGlobalMorph(this);
    }

    /// <summary>
    /// Modifies the hitbox of any active morph.<br/>
    /// This is only run after the morph's own ModifyHitbox, and short circuits as soon as any global returns false.
    /// </summary>
    /// <param name="morph">The morph being used.</param>
    /// <param name="player">The player using the morph.</param>
    /// <param name="size">The size of the hitbox.</param>
    /// <returns>If the hitbox was modified</returns>
    public virtual bool ModifyHitbox(Morph morph, Player player, ref Point16 size) => false;

    /// <summary>
    /// Called when any morph is activated.
    /// </summary>
    public virtual void OnMorph(Morph morph, Player player)
    {
    }

    /// <summary>
    /// Called when any morph is deactivated.
    /// </summary>
    public virtual void OnUnmorph(Morph morph, Player player)
    {
    }

    /// <inheritdoc cref="ModPlayer.ModifyDrawInfo(ref PlayerDrawSet)"/>
    public virtual void ModifyDrawInfo(Morph morph, ref PlayerDrawSet drawInfo)
    {
    }
}
