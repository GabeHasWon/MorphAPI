using MorphAPI.Core.Morphing;
using System;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;

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
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player has an active morph.</returns>
    /// <remarks>
    /// Use this for read checks in gameplay or drawing code on any side. It only reads the local copy of the morphed player's state.
    /// </remarks>
    public static bool HasMorph(this Player player) => player.GetModPlayer<MorphPlayer>().ActiveMorph is not null;

    /// <summary>
    /// Determines if the current player is in a <typeparamref name="T"/> morph.
    /// </summary>
    /// <typeparam name="T">The morph type to check for.</typeparam>
    /// <param name="player">The player to check.</param>
    /// <returns>True if the player's active morph is a <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Use this for read checks in gameplay or drawing code on any side. It only reads the local copy of the morphed player's state.
    /// </remarks>
    public static bool HasMorph<T>(this Player player) where T : Morph => player.GetModPlayer<MorphPlayer>().ActiveMorph is T;

    /// <summary>
    /// Retrieves the current morph for the player.
    /// </summary>
    /// <param name="player">The player using the morph.</param>
    /// <returns>The player's active morph, or null if the player is not morphed.</returns>
    /// <remarks>
    /// Use this for read checks in gameplay or drawing code on any side. The returned instance is the local copy of the morphed
    /// player's state, so mutating it only affects this side unless you sync the change yourself.
    /// </remarks>
    public static Morph? GetMorph(this Player player) => player.GetModPlayer<MorphPlayer>().ActiveMorph;

    /// <summary>
    /// Casts the player's morph to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The morph type to cast to.</typeparam>
    /// <param name="player">The player using the morph.</param>
    /// <returns>The player's active morph as a <typeparamref name="T"/>, or null if they are not morphed or are in a different morph.</returns>
    /// <remarks>
    /// Use this for read checks in gameplay or drawing code on any side. The returned instance is the local copy of the morphed
    /// player's state, so mutating it only affects this side unless you sync the change yourself.
    /// </remarks>
    public static T? GetMorph<T>(this Player player) where T : Morph => player.GetModPlayer<MorphPlayer>().ActiveMorph as T;

    /// <summary>
    /// Attempts to get the player's morph instance, if they have one.
    /// </summary>
    /// <param name="player">The player using the morph.</param>
    /// <param name="morph">The player's active morph, or null if the player is not morphed.</param>
    /// <returns>True if the player is morphed.</returns>
    /// <remarks>
    /// Use this for read checks in gameplay or drawing code on any side. It only reads the local copy of the morphed player's state.
    /// </remarks>
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
    /// Attempts to get the player's <typeparamref name="T"/> morph instance, if they have one.
    /// </summary>
    /// <typeparam name="T">The morph type to get.</typeparam>
    /// <param name="player">The player using the morph.</param>
    /// <param name="morph">The player's active morph as a <typeparamref name="T"/>, or null if they are not morphed or are in a different morph.</param>
    /// <returns>True if the player is morphed as a <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Use this for read checks in gameplay or drawing code on any side. It only reads the local copy of the morphed player's state.
    /// </remarks>
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
    /// Toggles the morph, either unmorphing or morphing depending on the player's state. This should be used to set/unset morphs.
    /// </summary>
    /// <param name="player">The player who's being modified.</param>
    /// <param name="newMorph">The morph applied if the player is not currently morphed.</param>
    /// <param name="fromNet">Whether this call is from net. Pass true when applying network or server state so no packet is sent.</param>
    /// <remarks>
    /// Use this from gameplay code, such as an item use effect or a keybind action, usually on <see cref="Main.LocalPlayer"/>.
    /// When <paramref name="fromNet"/> is false and the game is in multiplayer, the appropriate packet is sent automatically:
    /// a client sends the change to the server and a server broadcasts it; in singleplayer nothing is sent.
    /// </remarks>
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
    /// <param name="player">The player to unmorph.</param>
    /// <param name="fromNet">Whether this call is from net. Pass true when applying network or server state so no packet is sent.</param>
    /// <remarks>
    /// Use this to change a player's morph state from gameplay code. When <paramref name="fromNet"/> is false, a client automatically
    /// packets the change to the server and a server broadcasts it; in singleplayer nothing is sent. In packet handlers or when a
    /// server applies state directly, pass <paramref name="fromNet"/>: true so no packet is sent manually.
    /// </remarks>
    public static void Unmorph(this Player player, bool fromNet = false)
    {
        if (player.GetModPlayer<MorphPlayer>().ActiveMorph is { } oldMorph)
            MorphLoader.OnUnmorph(oldMorph, player);

        player.GetModPlayer<MorphPlayer>().ActiveMorph = null;

        if (!fromNet && Main.netMode != NetmodeID.SinglePlayer)
            MorphAPI.SendUnmorph(player);
    }

    /// <summary>
    /// Sets the player's morph to the given morph. This also dismounts the player if <see cref="Morph.BlockMounts"/> is true.<br/>
    /// This will unmorph the player first if they are currently in a morph.
    /// </summary>
    /// <param name="player">The player to morph.</param>
    /// <param name="newMorph">The morph to apply. Must not be null.</param>
    /// <param name="fromNet">Whether this call is from net. Pass true when applying network or server state so no packet is sent.</param>
    /// <remarks>
    /// Use this to change a player's morph state from gameplay code. When <paramref name="fromNet"/> is false, a client automatically
    /// packets the change to the server and a server broadcasts it; in singleplayer nothing is sent. In packet handlers or when a
    /// server applies state directly, pass <paramref name="fromNet"/>: true so no packet is sent manually.
    /// </remarks>
    public static void SetMorph(this Player player, Morph newMorph, bool fromNet = false)
    {
        ArgumentNullException.ThrowIfNull(newMorph);

        if (player.GetModPlayer<MorphPlayer>().ActiveMorph is not null)
            Unmorph(player, fromNet);

        player.GetModPlayer<MorphPlayer>().ActiveMorph = newMorph;

        if (newMorph.BlockMounts)
            player.mount.Dismount(player);

        MorphLoader.OnMorph(player.GetModPlayer<MorphPlayer>().ActiveMorph, player);

        if (!fromNet && Main.netMode != NetmodeID.SinglePlayer)
            MorphAPI.SendSetMorph(newMorph, player);
    }
}
