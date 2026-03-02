using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

internal class MorphLoader
{
    public delegate bool ModifyHitboxDelegate(Morph morph, Player player, ref Point16 size);
    public delegate void OnMorphDelegate(Morph morph, Player player);
    public delegate void OnUnmorphDelegate(Morph morph, Player player);
    public delegate void ModifyDrawInfoDelegate(Morph morph, ref PlayerDrawSet drawInfo);

    private static ModifyHitboxDelegate HookModifyHitbox = null;
    private static OnMorphDelegate HookOnMorph = null;
    private static OnUnmorphDelegate HookOnUnmorph = null;
    private static ModifyDrawInfoDelegate HookModifyDrawInfo = null;

    internal static void RegisterGlobalMorph(GlobalMorph morph)
    {
        HookModifyHitbox += morph.ModifyHitbox;
        HookOnMorph += morph.OnMorph;
        HookOnUnmorph += morph.OnUnmorph;
        HookModifyDrawInfo += morph.ModifyDrawInfo;
    }

    public static bool ModifyHitbox(Morph morph, Player player, out Point16 size)
    {
        if (morph.ModifyHitbox(player, out size))
        {
            foreach (var dele in HookModifyHitbox.GetInvocationList())
                if (!((ModifyHitboxDelegate)dele).Invoke(morph, player, ref size))
                    return false;

            return true;
        }

        return false;
    }

    public static void OnMorph(Morph morph, Player player)
    {
        morph.OnMorph(player);
        HookOnMorph.Invoke(morph, player);
    }

    public static void OnUnmorph(Morph morph, Player player)
    {
        morph.OnUnmorph(player);
        HookOnUnmorph.Invoke(morph, player);
    }

    public static void ModifyDrawInfo(Morph morph, ref PlayerDrawSet drawInfo)
    {
        morph.ModifyDrawInfo(ref drawInfo);
        HookModifyDrawInfo.Invoke(morph, ref drawInfo);
    }
}
