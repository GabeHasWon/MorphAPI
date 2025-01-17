using Terraria.DataStructures;
using Terraria.ID;

namespace MorphAPI.Core.Morphing;

internal class SampleMorph : Morph
{
    public override bool ModifyHitbox(Player player, out Point16 size)
    {
        size = new Point16(16, 16);
        return true;
    }

    public override void OnMorph(Player player)
    {
        for (int i = 0; i < 16; i++)
            Dust.NewDust(player.position, player.width, player.height, DustID.Dirt);
    }

    public override void OnUnmorph(Player player)
    {
        for (int i = 0; i < 16; i++)
            Dust.NewDust(player.position, player.width, player.height, DustID.Dirt);
    }
}
