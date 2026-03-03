using MorphAPI.Core.Morphing;
using System.Diagnostics.CodeAnalysis;

namespace MorphAPI.Core;

#nullable enable

/// <summary>
/// Various Morph-related extensions.
/// </summary>
public static class Extensions
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
    public static Morph? GetMorph(this Player player) => player.GetModPlayer<MorphPlayer>().ActiveMorph;

    /// <summary>
    /// Casts the player's morph to <typeparamref name="T"/>.
    /// </summary>
    public static T? GetMorph<T>(this Player player) where T : Morph => player.GetModPlayer<MorphPlayer>().ActiveMorph as T;

    /// <summary>
    /// Attempts to get the player's morph instance, if they have one, and returns true. Otherwise, returns false.
    /// </summary>
    public static bool TryGetMorph(this Player player, [NotNullWhen(true)] out Morph? morph)
    {
        if (player.GetModPlayer<MorphPlayer>().ActiveMorph is Morph tMorph)
        {
            morph = tMorph;
            return true;
        }

        morph = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the player's <typeparamref name="T"/> morph instance, if they have one, and returns true. Otherwise, returns false.
    /// </summary>
    public static bool TryGetMorph<T>(this Player player, [NotNullWhen(true)] out T? morph) where T : Morph
    {
        if (player.GetModPlayer<MorphPlayer>().ActiveMorph is T tMorph)
        {
            morph = tMorph;
            return true;
        }

        morph = null;
        return false;
    }

    /// <summary>
    /// Toggles the morph, either dismorphing or morphing depending on the player's state. This should be used to set/unset morphs.
    /// </summary>
    /// <param name="player">The player who's being modified.</param>
    /// <param name="newMorph">The morph that's being toggled.</param>
    /// <param name="fromNet">Whether this call is from net. This is unused by the API and is only included for parity.</param>
    public static void ToggleMorph(this Player player, Morph newMorph, bool fromNet = false)
    {
        if (player.HasMorph())
            Unmorph(player, fromNet);
        else
            SetMorph(player, newMorph, fromNet);
    }

    /// <summary>
    /// Unmorphs the player - calls <see cref="MorphLoader.OnUnmorph(Morph, Player)"/> and sets the morph to null.<br/>
    /// This does functionally nothing if the player is not morphed.
    /// </summary>
    public static void Unmorph(this Player player, bool fromNet = false)
    {
        if (player.GetModPlayer<MorphPlayer>().ActiveMorph is { } oldMorph)
            MorphLoader.OnUnmorph(oldMorph, player);

        player.GetModPlayer<MorphPlayer>().ActiveMorph = null;

        if (!fromNet && !Main.dedServ)
            MorphAPI.SendUnmorph(player);
    }

    /// <summary>
    /// Sets the player's morph to the given morph. This also dismounts the player if <see cref="Morph.BlockMounts"/> is true.<br/>
    /// This will unmorph the player if they are currently in a morph.<br/>
    /// <paramref name="fromNet"/> should not be true if you don't want to send a packet.
    /// </summary>
    public static void SetMorph(this Player player, Morph newMorph, bool fromNet = false)
    {
        if (player.GetModPlayer<MorphPlayer>().ActiveMorph is not null)
            Unmorph(player);

        player.GetModPlayer<MorphPlayer>().ActiveMorph = newMorph;

        if (newMorph.BlockMounts)
            player.mount.Dismount(player);

        MorphLoader.OnMorph(player.GetModPlayer<MorphPlayer>().ActiveMorph, player);

        if (!fromNet && !Main.dedServ)
            MorphAPI.SendSetMorph(newMorph, player);
    }
}
