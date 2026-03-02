using MorphAPI.Core.Morphing;

namespace MorphAPI.Core;

internal static class Extensions
{
    /// <summary>
    /// Determines if the player is currently morphed.
    /// </summary>
    public static bool HasMorph(this Player player) => player.GetModPlayer<MorphPlayer>().ActiveMorph is not null;

    /// <summary>
    /// Determines if the current player is in a <typeparamref name="T"/> morph.
    /// </summary>
    public static bool HasMorph<T>(this Player player) where T : Morph => player.GetModPlayer<MorphPlayer>().ActiveMorph is T;

    /// <summary>
    /// Retrieves the current morph for the player. Returns by reference, so you can modify the morph if desired.
    /// </summary>
    /// <param name="player">The player using the morph.</param>
    /// <returns></returns>
    public static Morph GetMorph(this Player player) => player.GetModPlayer<MorphPlayer>().ActiveMorph;

    /// <summary>
    /// Casts the player's morph to <typeparamref name="T"/>.
    /// </summary>
    public static T GetMorph<T>(this Player player) where T : Morph => player.GetModPlayer<MorphPlayer>().ActiveMorph as T;

    /// <summary>
    /// Toggles the morph, either dismorphing or morphing depending on the player's state. This should be used to set/unset morphs.
    /// </summary>
    /// <param name="player">The player who's being modified.</param>
    /// <param name="newMorph">The morph that's being toggled.</param>
    public static void ToggleMorph(this Player player, Morph newMorph)
    {
        Morph morph = player.GetMorph();

        if (player.HasMorph())
        {
            MorphLoader.OnUnmorph(morph, player);
            player.GetModPlayer<MorphPlayer>().ActiveMorph = null;
        }
        else
        {
            player.GetModPlayer<MorphPlayer>().ActiveMorph = newMorph;
            MorphLoader.OnMorph(player.GetModPlayer<MorphPlayer>().ActiveMorph, player);
        }
    }
}
