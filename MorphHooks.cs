using MorphAPI.Core;
using MorphAPI.Core.Morphing;
using Terraria.DataStructures;

namespace MorphAPI;

#nullable enable

internal class MorphHooks : ModSystem
{
    public readonly record struct OldMountData(Mount.MountData? OldData, int Height, bool OldActive);

    public override void Load()
    {
        On_Player.ResizeHitbox += EasilyModifyPlayerHeight;
        On_Player.QuickMount += CanMount;
    }

    private void CanMount(On_Player.orig_QuickMount orig, Player self)
    {
        if (self.GetMorph() is { } morph && morph.BlockMounts)
            return;

        orig(self);
    }

    /// <summary>
    /// Actually modifies the player's hitbox, with a very silly workaround.
    /// </summary>
    private static void EasilyModifyPlayerHeight(On_Player.orig_ResizeHitbox orig, Player self)
    {
        if (Main.gameMenu || self.mount is null || !self.HasMorph() || MorphLoader.ModifyHitbox(self.GetMorph(), self, out Point16 size))
        {
            orig(self);
            return;
        }

        bool isNull = self.mount.Type == -1; // Get old values
        int oldBoost = -1;
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

        if (!isNull)
            self.mount._data!.heightBoost = oldBoost;
        else
            self.mount.Reset();
    }
}
