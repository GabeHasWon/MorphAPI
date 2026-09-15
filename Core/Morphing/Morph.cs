using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

/// <summary>
/// Defines a morph, which includes drawing, movement, hitbox and usage.<br/>
/// Create your own by subclassing <see cref="Morph"/>: morphs are <see cref="ModType"/>s that are autoloaded, and one shared
/// instance is registered per type. <see cref="Clone"/> returns the per-player copy that is stored on <see cref="MorphPlayer.ActiveMorph"/>.
/// </summary>
/// <remarks>
/// Example usage:
/// <code>
/// public class MyMorph : Morph
/// {
///     public override bool ModifyHitbox(Player player, out Point16 size)
///     {
///         size = new Point16(16, 16);
///         return true;
///     }
/// }
/// </code>
/// </remarks>
public abstract class Morph : ModType
{
    private class MorphSetup : ModSystem
    {
        public override void PostSetupContent()
        {
            MorphsById = InternalMorphById.AsReadOnly();
            MorphNetIdByType = InternalMorphNetIdByType.AsReadOnly();
        }
    }

    internal static Dictionary<int, Morph> InternalMorphById = [];
    internal static Dictionary<Type, short> InternalMorphNetIdByType = [];

    /// <summary>
    /// Maps <see cref="NetId"/> values to morphs, primarily for networking. Populated during content setup; safe to read from
    /// <see cref="ModSystem.PostSetupContent"/> onward. This is a live read-only view of the registered morphs.
    /// </summary>
    public static ReadOnlyDictionary<int, Morph> MorphsById { get; private set; } = new(InternalMorphById);

    /// <summary>
    /// Lookup table for getting morph <see cref="NetId"/>s per type, avoiding the need for getting the singleton. Populated during
    /// content setup; safe to read from <see cref="ModSystem.PostSetupContent"/> onward. This is used for networking.
    /// </summary>
    public static ReadOnlyDictionary<Type, short> MorphNetIdByType { get; private set; } = new(InternalMorphNetIdByType);

    /// <summary>
    /// Number of morphs loaded.
    /// </summary>
    public static short MorphCount { get; private set; } = 0;

    /// <summary>
    /// Whether this morph hides the default draw layers entirely. Defaults to false.<br/>
    /// Set this to true to fully replace the vanilla player drawing; pair it with <see cref="SetDrawLayers(List{DrawData}, ref PlayerDrawSet)"/>
    /// and <see cref="PreClearDrawCache(ref PlayerDrawSet)"/>, using <see cref="PlayerDrawLayer.DrawWithTransformationAndChildren(ref PlayerDrawSet)"/>
    /// to draw the layers you want to keep.
    /// </summary>
    public virtual bool HideDefaultPlayer => false;

    /// <summary>
    /// Whether this morph disallows the use of mounts. Defaults to false.<br/>
    /// When true, the player is dismounted as they morph and quick-mounting is blocked while morphed.
    /// </summary>
    public virtual bool BlockMounts => false;

    /// <summary>
    /// The stable id used to identify this morph on the network. Assigned at registration.
    /// </summary>
    /// <remarks>
    /// Throws <see cref="KeyNotFoundException"/> for unregistered types, for example a morph that is not autoloaded.
    /// </remarks>
    public short NetId => MorphNetIdByType[GetType()];

    /// <summary>
    /// Registers this morph with the loader's lookup tables. Called by tModLoader when content loads; do not call this yourself.
    /// </summary>
    protected sealed override void Register()
    {
        ModTypeLookup<Morph>.Register(this);

        InternalMorphNetIdByType.Add(GetType(), MorphCount);
        InternalMorphById.Add(MorphCount, this);
        MorphCount++;
    }

    /// <summary>
    /// Clears the static morph lookup tables when the mod unloads.
    /// </summary>
    /// <remarks>
    /// Called once by <see cref="MorphAPI"/> during unload. Do not call this yourself.
    /// </remarks>
    internal static void UnloadStaticState()
    {
        InternalMorphById.Clear();
        InternalMorphNetIdByType.Clear();
        MorphCount = 0;
        MorphsById = new(InternalMorphById);
        MorphNetIdByType = new(InternalMorphNetIdByType);
    }

    /// <summary>
    /// Creates a per-player copy of this morph. The copy is shallow, so reference-type fields are shared between clones.
    /// </summary>
    public Morph Clone() => (Morph)MemberwiseClone();

    /// <summary>
    /// Modifies the hitbox if desired. The resulting hitbox will be of the resulting <paramref name="size"/> if this method returns true.<br/>
    /// This will do NOTHING if it returns false! Furthermore, this does not run when the player is mounted, as they have priority.
    /// </summary>
    /// <param name="player">The player who's hitbox is being modified.</param>
    /// <param name="size">Size of the new hitbox. Defaults to the player's default hitbox size.</param>
    /// <returns>True to apply <paramref name="size"/> as the hitbox; false to leave the vanilla hitbox untouched.</returns>
    /// <remarks>
    /// When this returns true, every <see cref="GlobalMorph"/> may still adjust or veto the resulting size.
    /// </remarks>
    public virtual bool ModifyHitbox(Player player, out Point16 size)
    {
        size = new Point16(Player.defaultWidth, Player.defaultHeight);
        return false;
    }

    /// <summary>
    /// Called when this morph is activated.
    /// </summary>
    /// <param name="player">The player being morphed.</param>
    /// <remarks>
    /// Runs on every side that applies the morph (server and clients), so guard client-only effects with <see cref="Main.dedServ"/>
    /// or net mode checks.
    /// </remarks>
    public virtual void OnMorph(Player player)
    {
    }

    /// <summary>
    /// Called when this morph is deactivated.
    /// </summary>
    /// <param name="player">The player being unmorphed.</param>
    /// <remarks>
    /// Runs on every side that applies the morph (server and clients), so guard client-only effects with <see cref="Main.dedServ"/>
    /// or net mode checks.
    /// </remarks>
    public virtual void OnUnmorph(Player player)
    {
    }

    /// <summary>
    /// Whether the given player, when morphed, can use this item. Returns true by default.
    /// </summary>
    /// <param name="player">The morphed player.</param>
    /// <param name="item">The item being used.</param>
    /// <returns>True to allow item usage; false to block it while morphed.</returns>
    /// <remarks>
    /// Runs through the <see cref="ModPlayer.CanUseItem(Item)"/> path as a local client item use gate.
    /// </remarks>
    public virtual bool CanUseItem(Player player, Item item) => true;

    /// <summary>
    /// Called every frame when this morph is active.
    /// </summary>
    /// <param name="player">The morphed player.</param>
    /// <remarks>
    /// Called from <see cref="MorphPlayer.UpdateEquips"/>, which runs on all sides including a dedicated server, so do not assume
    /// there is a client.
    /// </remarks>
    public virtual void Update(Player player)
    {
    }

    /// <summary>
    /// Allows this morph to modify the player's drawing.
    /// </summary>
    /// <param name="drawInfo">The player's draw info.</param>
    /// <remarks>
    /// Only runs on clients, as part of the drawing flow.
    /// </remarks>
    public virtual void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
    }

    /// <summary>
    /// Runs before the draw data cache is cleared when <see cref="HideDefaultPlayer"/> is true, allowing morphs to retain certain draw data.
    /// </summary>
    /// <param name="drawInfo">The player's draw info.</param>
    /// <returns>The draw data to keep; an empty list by default.</returns>
    /// <remarks>
    /// Only runs on clients. The returned data is passed to <see cref="SetDrawLayers(List{DrawData}, ref PlayerDrawSet)"/>.
    /// </remarks>
    public virtual List<DrawData> PreClearDrawCache(ref PlayerDrawSet drawInfo) => [];

    /// <summary>
    /// Allows modification of draw layers, including the draw layers preserved from <see cref="PreClearDrawCache(ref PlayerDrawSet)"/>.
    /// </summary>
    /// <param name="oldDrawData">Preserved layers from <see cref="PreClearDrawCache(ref PlayerDrawSet)"/>.</param>
    /// <param name="drawInfo">The player's draw info.</param>
    /// <remarks>
    /// Only runs on clients, and only when <see cref="HideDefaultPlayer"/> is true. Re-add the layers you want to keep by drawing
    /// them with <see cref="PlayerDrawLayer.DrawWithTransformationAndChildren(ref PlayerDrawSet)"/>.
    /// </remarks>
    public virtual void SetDrawLayers(List<DrawData> oldDrawData, ref PlayerDrawSet drawInfo)
    {
    }

    /// <summary>
    /// Allows you to add additional information to sync when the <see cref="MorphAPI.MessageID.SetMorph"/> and <see cref="MorphAPI.MessageID.UpdateMorph"/> packets are sent.<br/>
    /// Make sure to receive the information in the same order in <see cref="NetRecieve(BinaryReader)"/>.
    /// </summary>
    /// <param name="writer">The packet writer to write to.</param>
    /// <remarks>
    /// Runs on the sender's client and, for join sync, on the server, so never assume the local player is the morph's owner. The
    /// write/read order must stay symmetric.
    /// </remarks>
    public virtual void NetSend(BinaryWriter writer)
    {
    }

    /// <summary>
    /// Receives the information written in <see cref="NetSend(BinaryWriter)"/>.
    /// </summary>
    /// <param name="reader">The packet reader to read from, positioned as the sender wrote it.</param>
    /// <remarks>
    /// Runs on the server for state sent by a client, and on remote clients for relays, but never on the client that originally sent
    /// the state. Read fields in the same order <see cref="NetSend(BinaryWriter)"/> wrote them, and do not assume the morph belongs to
    /// the local player. The method name's spelling is historical and is retained.
    /// </remarks>
    public virtual void NetRecieve(BinaryReader reader)
    {
    }
}
