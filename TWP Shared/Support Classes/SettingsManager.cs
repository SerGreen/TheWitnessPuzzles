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
        public static bool BloomFX { get; set; } = true;
        public static float Sensitivity { get; set; } = 1.0f;

#if ANDROID
        public static DisplayOrientation ScreenOrientation { get; set; } = DisplayOrientation.Default;
        public static event Action OrientationLockChanged;
        private static bool _isOrientationLocked = false;
        public static bool IsOrientationLocked
        {
            get => _isOrientationLocked;
            set
            {
                if(value != _isOrientationLocked)
                {
                    _isOrientationLocked = value;
                    OrientationLockChanged?.Invoke();
                }
            }
        }
#endif

        public static void SaveSettings()
        {
            StringBuilder file = new StringBuilder();
            file.Append(IsMute ? 1 : 0).Append(":");
            file.Append((int) (MasterVolume * 1000)).Append(":");
            file.Append(IsFullscreen ? 1 : 0).Append(":");
            file.Append(BloomFX ? 1 : 0).Append(":");
            file.Append((int) (Sensitivity * 1000));

#if ANDROID
            file.Append(":");
            file.Append(IsOrientationLocked ? 1 : 0).Append(":");
            file.Append((int) ScreenOrientation);
#endif

            FileStorageManager.SaveSettingsFile(file.ToString());
        }

        public static void LoadSettings()
        {
            string file = FileStorageManager.LoadSettingsFile();
            if(file != null)
            {
                int[] data = Array.ConvertAll(file.Split(":".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), int.Parse);
                if (data.Length >= 5)
                {
                    IsMute = data[0] == 1;
                    MasterVolume = data[1] / 1000f;  // 0..1000 interval into float 0.0 .. 1.0
                    IsFullscreen = data[2] == 1;
                    BloomFX = data[3] == 1;
                    Sensitivity = data[4] / 1000f;
#if ANDROID
                    if(data.Length >= 7)
                    {
                        _isOrientationLocked = data[5] == 1;
                        ScreenOrientation = (DisplayOrientation) data[6];
                    }
#endif
                }
            }
        }
    }
}
