using Terraria.DataStructures;

namespace MorphAPI.Core.Morphing;

/// <summary>
/// Internal pipeline that runs the active morph's own hooks first, then every registered <see cref="GlobalMorph"/> hook.
/// Mods do not use this directly; use <see cref="Morph"/> and <see cref="GlobalMorph"/> instead.
/// </summary>
internal class MorphLoader
{
    /// <summary>
    /// Delegate for the global morph hitbox hook. Return true to allow the given size, false to veto it.
    /// </summary>
    public delegate bool ModifyHitboxDelegate(Morph morph, Player player, ref Point16 size);

    /// <summary>
    /// Delegate for the global morph activation hook.
    /// </summary>
    public delegate void OnMorphDelegate(Morph morph, Player player);

    /// <summary>
    /// Delegate for the global morph deactivation hook.
    /// </summary>
    public delegate void OnUnmorphDelegate(Morph morph, Player player);

    /// <summary>
    /// Delegate for the global morph draw info hook.
    /// </summary>
    public delegate void ModifyDrawInfoDelegate(Morph morph, ref PlayerDrawSet drawInfo);

    private static ModifyHitboxDelegate HookModifyHitbox = null;
    private static OnMorphDelegate HookOnMorph = null;
    private static OnUnmorphDelegate HookOnUnmorph = null;
    private static ModifyDrawInfoDelegate HookModifyDrawInfo = null;

    /// <summary>
    /// Subscribes a global morph's hooks. Called automatically when a <see cref="GlobalMorph"/> is registered; do not call this yourself.
    /// </summary>
    internal static void RegisterGlobalMorph(GlobalMorph morph)
    {
        HookModifyHitbox += morph.ModifyHitbox;
        HookOnMorph += morph.OnMorph;
        HookOnUnmorph += morph.OnUnmorph;
        HookModifyDrawInfo += morph.ModifyDrawInfo;
    }

    /// <summary>
    /// Runs the hitbox pipeline: first the morph decides its own size, then each global morph may adjust or veto it.
    /// </summary>
    /// <param name="morph">The active morph.</param>
    /// <param name="player">The morphed player.</param>
    /// <param name="size">The resulting hitbox size.</param>
    /// <returns>True if a morph hitbox should be applied, false to keep the vanilla hitbox.</returns>
    public static bool ModifyHitbox(Morph morph, Player player, out Point16 size)
    {
        if (!morph.ModifyHitbox(player, out size))
            return false;

        if (HookModifyHitbox is null)
            return true;

        foreach (var dele in HookModifyHitbox.GetInvocationList())
            if (!((ModifyHitboxDelegate)dele).Invoke(morph, player, ref size))
                return false;

        return true;
    }

    /// <summary>
    /// Runs the morph's own <see cref="Morph.OnMorph(Player)"/> and then every global morph's <see cref="GlobalMorph.OnMorph(Morph, Player)"/>.
    /// </summary>
    public static void OnMorph(Morph morph, Player player)
    {
        morph.OnMorph(player);
        HookOnMorph?.Invoke(morph, player);
    }

    /// <summary>
    /// Runs the morph's own <see cref="Morph.OnUnmorph(Player)"/> and then every global morph's <see cref="GlobalMorph.OnUnmorph(Morph, Player)"/>.
    /// </summary>
    public static void OnUnmorph(Morph morph, Player player)
    {
        morph.OnUnmorph(player);
        HookOnUnmorph?.Invoke(morph, player);
    }

    /// <summary>
    /// Runs the morph's own <see cref="Morph.ModifyDrawInfo(ref PlayerDrawSet)"/> and then every global morph's <see cref="GlobalMorph.ModifyDrawInfo(Morph, ref PlayerDrawSet)"/>.
    /// </summary>
    public static void ModifyDrawInfo(Morph morph, ref PlayerDrawSet drawInfo)
    {
        morph.ModifyDrawInfo(ref drawInfo);
        HookModifyDrawInfo?.Invoke(morph, ref drawInfo);
    }

    /// <summary>
    /// Clears all global morph hook subscriptions.
    /// </summary>
    /// <remarks>
    /// Called once by <see cref="MorphAPI"/> when the mod unloads. Do not call this yourself.
    /// </remarks>
    internal static void Unload()
    {
        HookModifyHitbox = null;
        HookOnMorph = null;
        HookOnUnmorph = null;
        HookModifyDrawInfo = null;
    }
}
