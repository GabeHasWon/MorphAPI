global using Terraria.ModLoader;
global using Terraria;
global using Microsoft.Xna.Framework;
using MorphAPI.Core.Morphing;
using System.IO;
using MorphAPI.Core;
using System;
using Terraria.ID;

namespace MorphAPI;

#nullable enable

/// <summary>
/// The mod entry point for MorphAPI. It owns the morph network protocol and the static morph state that is cleaned up when the mod unloads.
/// </summary>
public class MorphAPI : Mod
{
    /// <summary>
    /// The internal wire protocol message ids used by MorphAPI. These are part of the network format, so do not renumber them.
    /// </summary>
    public enum MessageID : byte
    {
        /// <summary>
        /// Sets the player's morph.
        /// </summary>
        SetMorph = 0,

        /// <summary>
        /// Updates the player's morph by calling <see cref="Morph.NetSend(BinaryWriter)"/> and <see cref="Morph.NetRecieve(BinaryReader)"/>.
        /// </summary>
        UpdateMorph = 1,

        /// <summary>
        /// Unmorphs the player.
        /// </summary>
        Unmorph = 2
    }

    /// <summary>
    /// Sends a <see cref="MessageID.SetMorph"/> packet to sync a player's morph.
    /// </summary>
    /// <param name="morph">The morph to sync. Must not be null.</param>
    /// <param name="player">The player the morph belongs to. Must not be null.</param>
    /// <param name="toClient">The client index to send to, or -1 to send to every client.</param>
    /// <param name="ignoreClient">The client index to skip, or -1 to skip no client.</param>
    /// <remarks>
    /// You normally do not call this directly; the <see cref="Extensions"/> methods send it for you. This is a no-op in singleplayer.
    /// On a server it broadcasts the morph to the clients (or uses <paramref name="toClient"/> and <paramref name="ignoreClient"/>);
    /// on a client the packet is sent to the server, which attributes it to the sending player and relays it, ignoring
    /// <paramref name="toClient"/> and <paramref name="ignoreClient"/>. <see cref="Morph.NetSend(BinaryWriter)"/> runs on the caller's side.
    /// </remarks>
    public static void SendSetMorph(Morph morph, Player player, int toClient = -1, int ignoreClient = -1)
    {
        ArgumentNullException.ThrowIfNull(morph);
        ArgumentNullException.ThrowIfNull(player);

        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = GetPacket(MessageID.SetMorph, 255);
        packet.Write(morph.NetId);

        if (Main.dedServ)
            packet.Write((byte)player.whoAmI);

        morph.NetSend(packet);
        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    /// Sends a <see cref="MessageID.Unmorph"/> packet to sync a player's unmorph.
    /// </summary>
    /// <param name="player">The player to unmorph. Must not be null.</param>
    /// <param name="toClient">The client index to send to, or -1 to send to every client.</param>
    /// <param name="ignoreClient">The client index to skip, or -1 to skip no client.</param>
    /// <remarks>
    /// You normally do not call this directly; <see cref="Core.Extensions.Unmorph(Player, bool)"/> sends it for you. This is a no-op in
    /// singleplayer. On a server it broadcasts the unmorph to the clients (or uses <paramref name="toClient"/> and
    /// <paramref name="ignoreClient"/>); on a client the packet is sent to the server, which attributes it to the sending player and
    /// relays it, ignoring <paramref name="toClient"/> and <paramref name="ignoreClient"/>.
    /// </remarks>
    public static void SendUnmorph(Player player, int toClient = -1, int ignoreClient = -1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = GetPacket(MessageID.Unmorph, 2);

        if (Main.dedServ)
            packet.Write((byte)player.whoAmI);
        
        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    /// Sends a <see cref="MessageID.UpdateMorph"/> packet to sync a player's current morph state.
    /// </summary>
    /// <param name="player">The player whose morph is being updated. Must not be null.</param>
    /// <param name="toClient">The client index to send to, or -1 to send to every client.</param>
    /// <param name="ignoreClient">The client index to skip, or -1 to skip no client.</param>
    /// <remarks>
    /// You normally do not call this directly; call it after changing state that <see cref="Morph.NetSend(BinaryWriter)"/> writes so
    /// other players receive the change. This is a no-op in singleplayer.
    /// On a server it broadcasts the state to the clients (or uses <paramref name="toClient"/> and <paramref name="ignoreClient"/>);
    /// on a client the packet is sent to the server, which attributes it to the sending player and relays it, ignoring
    /// <paramref name="toClient"/> and <paramref name="ignoreClient"/>. <see cref="Morph.NetSend(BinaryWriter)"/> runs on the caller's side.
    /// </remarks>
    public static void SendUpdateMorph(Player player, int toClient = -1, int ignoreClient = -1)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = GetPacket(MessageID.UpdateMorph, 255);

        if (Main.dedServ)
            packet.Write((byte)player.whoAmI);

        MorphPlayer modPlayer = player.GetModPlayer<MorphPlayer>();
        bool hasMorph = modPlayer.ActiveMorph is not null;
        packet.Write(hasMorph);

        if (modPlayer.ActiveMorph is { } morph)
            morph.NetSend(packet);

        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    /// Creates a <see cref="ModPacket"/> pre-written with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The <see cref="MessageID"/> to write as the first byte of the packet.</param>
    /// <param name="capacity">A buffer-size hint only; the packet grows as needed.</param>
    /// <returns>The packet, with the message id already written.</returns>
    /// <remarks>
    /// Callers must write their payload in the same order that <see cref="HandlePacket(BinaryReader, int)"/> reads it.
    /// </remarks>
    public static ModPacket GetPacket(MessageID id, byte capacity = 255)
    {
        ModPacket packet = ModContent.GetInstance<MorphAPI>().GetPacket(capacity);
        packet.Write((byte)id);
        return packet;
    }

    /// <summary>
    /// Handles packets sent by clients and the server.
    /// </summary>
    /// <param name="reader">The packet data, positioned after the message id.</param>
    /// <param name="whoAmI">The index of the player that sent the packet.</param>
    /// <remarks>
    /// Called by tModLoader for every packet addressed to this mod; never call it manually. Packets with an unknown morph or an
    /// invalid player index are logged and ignored.
    /// </remarks>
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        var id = (MessageID)reader.ReadByte();

        if (id == MessageID.SetMorph)
        {
            short morphId = reader.ReadInt16();
            int player = Main.dedServ ? whoAmI : reader.ReadByte();

            if (!Morph.InternalMorphById.TryGetValue(morphId, out Morph? morph))
            {
                Logger.Warn($"Received a SetMorph packet with an unknown morph id: {morphId}");
                return;
            }

            if (player >= Main.maxPlayers)
            {
                Logger.Warn($"Received a morph packet for an invalid player index: {player}");
                return;
            }

            morph = morph.Clone();
            morph.NetRecieve(reader);

            Main.player[player].SetMorph(morph, true);

            if (Main.dedServ)
                SendSetMorph(morph, Main.player[player], -1, player);
        }
        else if (id == MessageID.UpdateMorph)
        {
            int player = Main.dedServ ? whoAmI : reader.ReadByte();
            bool hasMorph = reader.ReadBoolean();

            if (player >= Main.maxPlayers)
            {
                Logger.Warn($"Received a morph packet for an invalid player index: {player}");
                return;
            }

            if (hasMorph && Main.player[player].GetModPlayer<MorphPlayer>().ActiveMorph is { } activeMorph)
                activeMorph.NetRecieve(reader);

            if (Main.dedServ)
                SendUpdateMorph(Main.player[player], -1, player);
        }
        else if (id == MessageID.Unmorph)
        {
            int player = Main.dedServ ? whoAmI : reader.ReadByte();

            if (player >= Main.maxPlayers)
            {
                Logger.Warn($"Received a morph packet for an invalid player index: {player}");
                return;
            }

            Main.player[player].Unmorph(true);

            if (Main.dedServ)
                SendUnmorph(Main.player[player], -1, player);
        }
    }

    /// <summary>
    /// Cleans up the static morph state and hook delegates when the mod unloads.
    /// </summary>
    /// <remarks>
    /// Called once by tModLoader during mod unloading. Do not call this yourself.
    /// </remarks>
    public override void Unload()
    {
        Morph.UnloadStaticState();
        MorphLoader.Unload();
    }
}
