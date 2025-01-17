using MorphAPI.Core.Morphing;

namespace MorphAPI.Core;

internal static class Extensions
{
    public static bool HasMorph(this Player player) => player.GetModPlayer<MorphPlayer>().ActiveMorph is not null;
    public static ref Morph GetMorph(this Player player) => ref player.GetModPlayer<MorphPlayer>().ActiveMorph;

    public static void ToggleMorph(this Player player, Morph newMorph)
    {
        ref Morph morph = ref player.GetMorph();

        if (player.HasMorph())
        {
            morph.OnUnmorph(player);
            morph = null;
        }
        else
        {
            morph = newMorph;
            morph.OnMorph(player);
        }
    }
}
