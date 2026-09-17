using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

internal class MorphLoader : ILoadable
{
    public delegate bool ModifyHitboxDelegate(Morph morph, Player player, ref Point16 size);
    public delegate void OnMorphDelegate(Morph morph, Player player);
    public delegate void OnUnmorphDelegate(Morph morph, Player player);
    public delegate void ModifyDrawInfoDelegate(Morph morph, ref PlayerDrawSet drawInfo);

    private static ModifyHitboxDelegate HookModifyHitbox = null;
    private static OnMorphDelegate HookOnMorph = null;
    private static OnUnmorphDelegate HookOnUnmorph = null;
    private static ModifyDrawInfoDelegate HookModifyDrawInfo = null;

    void ILoadable.Load(Mod mod) { }

    void ILoadable.Unload()
    {
        HookModifyDrawInfo = null;
        HookOnMorph = null;
        HookOnUnmorph = null;
        HookModifyHitbox = null;
    }

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

    /// <summary>
    /// Called when any player uses any morph. Will always call the current morph's OnMorph, then do globals.
    /// </summary>
    public static void OnMorph(Morph morph, Player player)
    {
        morph.OnMorph(player);
        HookOnMorph.Invoke(morph, player);
    }

    /// <summary>
    /// Called when any player stops using any morph. Will always call the current morph's OnUnmorph, then do globals.
    /// </summary>
    public static void OnUnmorph(Morph morph, Player player)
    {
        morph.OnUnmorph(player);
        HookOnUnmorph.Invoke(morph, player);
    }

    /// <summary>
    /// Modifies the player's draw info based on first the current morph and then the second, if any.
    /// </summary>
    /// <param name="morph"></param>
    /// <param name="drawInfo"></param>
    public static void ModifyDrawInfo(Morph morph, ref PlayerDrawSet drawInfo)
    {
        morph.ModifyDrawInfo(ref drawInfo);
        HookModifyDrawInfo.Invoke(morph, ref drawInfo);
    }
}
