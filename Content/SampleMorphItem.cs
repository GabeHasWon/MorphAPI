using MorphAPI.Core;
using Terraria.ID;

namespace MorphAPI.Content;

/// <summary>
/// Simple item that solely allows the player to morph into <see cref="SampleMorph"/>.<br/>
/// Make sure to remove the Autoload attribute (or set EnableMorph to true) if you want to use this!
/// </summary>
[Autoload(SampleMorph.EnableMorph)]
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
