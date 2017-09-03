using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TWP_Shared
{
    public static class SettingsManager
    {
        public static float MasterVolume
        {
            get => SoundManager.MasterVolume;
            set => SoundManager.MasterVolume = value;
        }

        public static bool IsMute
        {
            get => SoundManager.Mute;
            set => SoundManager.Mute = value;
        }

        public static bool IsFullscreen { get; set; } = false;
        public static bool VFX { get; set; } = true;

        public static void SaveSettings()
        {
            StringBuilder file = new StringBuilder();
            file.Append(IsMute ? 1 : 0).Append(":");
            file.Append(MasterVolume * 10).Append(":");
            file.Append(IsFullscreen ? 1 : 0).Append(":");
            file.Append(VFX ? 1 : 0);

            FileStorageManager.SaveSettingsFile(file.ToString());
        }

        public static void LoadSettings()
        {
            string file = FileStorageManager.LoadSettingsFile();
            if(file != null)
            {
                int[] data = Array.ConvertAll(file.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), int.Parse);
                if (data.Length == 4)
                {
                    IsMute = data[0] == 1;
                    MasterVolume = data[1] / 10f;  // 0..10 interval into float 0.0 .. 1.0
                    IsFullscreen = data[2] == 1;
                    VFX = data[3] == 1;
                }
            }
        }
    }
}
