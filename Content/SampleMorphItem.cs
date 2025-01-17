using MorphAPI.Core;
using MorphAPI.Core.Morphing;
using Terraria.ID;

namespace MorphAPI.Content;

internal class SampleMorphItem : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new Vector2(20);
        Item.useTime = Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
    }

    public override bool? UseItem(Player player)
    {
        player.ToggleMorph(new SampleMorph());
        return true;
    }
}
