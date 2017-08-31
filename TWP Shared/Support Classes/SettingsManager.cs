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

        }

        public static void LoadSettings()
        {

        }
    }
}
