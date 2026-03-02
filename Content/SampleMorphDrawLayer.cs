using Microsoft.Xna.Framework.Graphics;
using MorphAPI.Core;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace MorphAPI.Content;

/// <summary>
/// Defines the sample draw layer for the <see cref="SampleMorph"/> dirt block morph.<br/>
/// Make sure to remove the Autoload attribute (or set EnableMorph to true) if you want to use this!
/// </summary>
[Autoload(SampleMorph.EnableMorph)]
public class SampleMorphDrawLayer : PlayerDrawLayer
{
    /// <summary>
    /// Defaults to the front of the player; doesn't really matter, as this layer replaces all others.
    /// </summary>
    public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ArmOverItem);

    /// <summary>
    /// Draws the dirt block.
    /// </summary>
    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;

        if (player.HasMorph<SampleMorph>())
        {
            Texture2D tex = TextureAssets.Item[ModContent.ItemType<SampleMorphItem>()].Value;
            Color color = Lighting.GetColor(player.Center.ToTileCoordinates());
            Vector2 pos = player.Bottom - Main.screenPosition + new Vector2(0, player.gfxOffY - 10);
            
            var data = new DrawData(tex, pos.Floor(), null, color, player.GetMorph<SampleMorph>().rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
            drawInfo.DrawDataCache.Add(data);
        }
    }
}
