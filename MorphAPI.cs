global using Terraria.ModLoader;
global using Terraria;
global using Microsoft.Xna.Framework;
using MorphAPI.Core.Morphing;
using System.IO;
using MorphAPI.Core;
using NVorbis.Contracts;
using System;
using System.Diagnostics;

namespace MorphAPI;

/// <summary>
/// Used to manage packets.
/// </summary>
public class MorphAPI : Mod
{
    /// <summary>
    /// Defines packet ids for MorphAPI.
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
        Unmorph = 2,

        /// <summary>
        /// Requests all other morphs from every other player.
        /// </summary>
        RequestMorphs,
    }

    /// <summary>
    /// Syncs the player's morph with the server and every other client.
    /// </summary>
    public static void SendSetMorph(Morph morph, Player player, int toClient = -1, int ignoreClient = -1)
    {
        ModPacket packet = GetPacket(MessageID.SetMorph, 255);
        packet.Write(morph.NetId);

        if (Main.dedServ)
            packet.Write((byte)player.whoAmI);

        morph.NetSend(packet);
        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    /// Syncs the player's unmorph with the server and every other client.
    /// </summary>
    public static void SendUnmorph(Player player, int toClient = -1, int ignoreClient = -1)
    {
        ModPacket packet = GetPacket(MessageID.Unmorph, 2);

        if (Main.dedServ)
            packet.Write((byte)player.whoAmI);
        
        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    /// Requests all players to send their morphs to the current player from the server. This should only be sent from a client.
    /// </summary>
    public static void RequestServerMorphs(int serverWho = -1)
    {
        ModPacket packet = GetPacket(MessageID.RequestMorphs, 2);

        if (Main.dedServ)
        {
            Debug.Assert(serverWho != -1, "A player whoAmI must be provided for a RequestMorphs message.");
            packet.Write((byte)serverWho);
        }

        packet.Send();
    }

    /// <summary>
    /// Sends a <see cref="MessageID.UpdateMorph"/> packet.
    /// </summary>
    public static void SendUpdateMorph(Player player, int toClient = -1, int ignoreClient = -1)
    {
        ModPacket packet = GetPacket(MessageID.UpdateMorph, 255);

        if (Main.dedServ)
            packet.Write((byte)player.whoAmI);

        bool hasMorph = player.GetModPlayer<MorphPlayer>().ActiveMorph is not null;
        packet.Write(hasMorph);

        if (hasMorph)
            player.GetModPlayer<MorphPlayer>().ActiveMorph.NetSend(packet);

        packet.Send(toClient, ignoreClient);
    }

    /// <summary>
    /// Sets up a packet that sends the given message ID.<para/>
    /// <b>Note:</b> You likely want to use one of the pre-built packets, such as <see cref="SendSetMorph(Morph, Player, int, int)"/> 
    /// or <see cref="SendUnmorph(Player, int, int)"/>, instead of using this packet. If you have a 
    /// </summary>
    public static ModPacket GetPacket(MessageID id, byte capacity = 255)
    {
        ModPacket packet = ModContent.GetInstance<MorphAPI>().GetPacket(capacity);
        packet.Write((byte)id);
        return packet;
    }

    /// <summary>
    /// Handles all packets.
    /// </summary>
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        var id = (MessageID)reader.ReadByte();

        if (id == MessageID.SetMorph)
        {
            short morphId = reader.ReadInt16();
            byte player = Main.dedServ ? (byte)whoAmI : reader.ReadByte();
            Morph morph = Morph.InternalMorphById[morphId].Clone();
            morph.NetRecieve(reader);

            Main.player[player].SetMorph(morph, true);

            if (Main.dedServ)
                SendSetMorph(morph, Main.player[player], -1, player);
        }
        else if (id == MessageID.UpdateMorph)
        {
            byte player = Main.dedServ ? (byte)whoAmI : reader.ReadByte();
            bool hasMorph = reader.ReadBoolean();

            if (hasMorph)
                Main.player[player].GetModPlayer<MorphPlayer>().ActiveMorph.NetRecieve(reader);

            if (Main.dedServ)
                SendUpdateMorph(Main.player[player], -1, player);
        }
        else if (id == MessageID.Unmorph)
        {
            byte player = Main.dedServ ? (byte)whoAmI : reader.ReadByte();
            Main.player[player].Unmorph(true);

            if (Main.dedServ)
                SendUnmorph(Main.player[player], -1, player);
        }
        else if (id == MessageID.RequestMorphs)
        {
            if (Main.dedServ)
                RequestServerMorphs(whoAmI);
            else
            {
                byte requestee = reader.ReadByte();
                Player player = Main.LocalPlayer;

                if (player.TryGetMorph(out Morph morph))
                    SendSetMorph(morph, player, requestee);
            }
        }
    }
}
