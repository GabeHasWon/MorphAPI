using MorphAPI.Core;
using Terraria.DataStructures;

namespace MorphAPI;

internal class MorphHooks : ModSystem
{
    public override void Load() => On_Player.ResizeHitbox += EasilyModifyPlayerHeight;

    private static void EasilyModifyPlayerHeight(On_Player.orig_ResizeHitbox orig, Player self)
    {
        if (Main.gameMenu || self.mount is null || !self.HasMorph() || !self.GetMorph().ModifyHitbox(self, out Point16 size))
        {
            orig(self);
            return;
        }

        bool isNull = self.mount.Type == -1; // Get old values
        int oldBoost = -1;
        bool resetData = self.mount._data is null;
        bool oldActive = self.mount._active;
        
        if (resetData) // Force mount to be active
            self.mount._data = new Mount.MountData();

        int offset = size.Y - Player.defaultHeight;

        self.mount._data.heightBoost = offset;
        self.mount._active = true;

        orig(self);

        self.width = size.X; // Set width
        self.mount._active = oldActive; // Unset all old values

        if (resetData)
            self.mount._data = null;

        if (!isNull)
            self.mount._data.heightBoost = oldBoost;
        else
            self.mount.Reset();
    }
}
