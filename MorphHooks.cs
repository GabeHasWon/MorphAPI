using MorphAPI.Core;
using MorphAPI.Core.Morphing;
using Terraria.DataStructures;

namespace MorphAPI;

#nullable enable

/// <summary>
/// Applies morph hitboxes and mount restrictions to players through tModLoader detours.
/// </summary>
internal class MorphHooks : ModSystem
{
    /// <summary>
    /// Snapshot of the mount data that had to be temporarily replaced while a morph hitbox is applied.
    /// </summary>
    public readonly record struct OldMountData(Mount.MountData? OldData, int Height, bool OldActive);

    public override void Load()
    {
        On_Player.ResizeHitbox += EasilyModifyPlayerHeight;
        On_Player.QuickMount += CanMount;
    }

    /// <summary>
    /// Removes the hitbox and quick-mount hooks.
    /// </summary>
    /// <remarks>
    /// Called once by tModLoader when the mod unloads. Do not call this yourself.
    /// </remarks>
    public override void Unload()
    {
        On_Player.ResizeHitbox -= EasilyModifyPlayerHeight;
        On_Player.QuickMount -= CanMount;
    }

    private void CanMount(On_Player.orig_QuickMount orig, Player self)
    {
        if (self.GetMorph() is { } morph && morph.BlockMounts)
            return;

        orig(self);
    }

    /// <summary>
    /// Actually modifies the player's hitbox, with a very silly workaround.<br/>
    /// The workaround temporarily fakes the player's mount data and height boost so that vanilla hitbox resizing applies the
    /// morph's size, then restores the original mount data, height boost and active state. Mounted players are skipped entirely, so they keep
    /// their mount's own hitbox.
    /// </summary>
    private static void EasilyModifyPlayerHeight(On_Player.orig_ResizeHitbox orig, Player self)
    {
        if (Main.gameMenu || self.mount.Active || !self.TryGetMorph(out Morph? morph) || !MorphLoader.ModifyHitbox(morph, self, out Point16 size))
        {
            self.width = Player.defaultWidth;
            orig(self);
            return;
        }

        bool isNull = self.mount.Type == -1; // Get old values
        int oldBoost = self.mount._data?.heightBoost ?? -1;
        bool resetData = self.mount._data is null;
        bool oldActive = self.mount._active;

        OldMountData? mountData = null;

        if (resetData) // Force mount to be active
            self.mount._data = new Mount.MountData();
        else if (self.mount._data is not null)
            mountData = new OldMountData(self.mount._data, self.mount._data.heightBoost, self.mount.Active);

        int offset = size.Y - Player.defaultHeight;

        self.mount._data!.heightBoost = offset;
        self.mount._active = true;

        orig(self);

        self.width = size.X; // Set width
        self.mount._active = oldActive; // Unset all old values

        if (resetData)
            self.mount._data = null;
        else if (mountData is { } oldData)
        {
            self.mount._data = oldData.OldData;
            self.mount._active = oldData.OldActive;
            self.mount._data!.heightBoost = oldData.Height;
        }

        if (!isNull && self.mount._data is not null)
            self.mount._data.heightBoost = oldBoost;
        else if (isNull)
            self.mount.Reset();
    }
}
