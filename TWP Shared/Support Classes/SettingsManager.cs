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

        public static bool IsFullscreen { get; set; }
#if ANDROID
                                                        = true;     // On Android fullscreen is On by default
#else
                                                        = false;    // On Windows fullscreen is Off by default
#endif
        public static bool BloomFX { get; set; } = false;
        public static float Sensitivity { get; set; } = 1.0f;

        // Sequential mode settings
        public static bool IsSequentialMode { get; set; } = false;
        public static int CurrentSequentialSeed { get; set; } = 0;

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
            file.Append("mute = ").Append(IsMute ? 1 : 0).Append("\n");
            file.Append("volume = ").Append(MasterVolume).Append("\n");
            file.Append("fullscreen = ").Append(IsFullscreen ? 1 : 0).Append("\n");
            file.Append("bloom = ").Append(BloomFX ? 1 : 0).Append("\n");
            file.Append("sensitivity = ").Append(Sensitivity).Append("\n");

#if ANDROID
            file.Append("orientationLock = ").Append(IsOrientationLocked ? 1 : 0).Append("\n");
            file.Append("orientation = ").Append((int) ScreenOrientation).Append("\n");
#endif

            file.Append("sequentialMode = ").Append(IsSequentialMode ? 1 : 0).Append("\n");
            file.Append("sequentialSeed = ").Append(CurrentSequentialSeed).Append("\n");

            FileStorageManager.SaveSettingsFile(file.ToString());
        }

        public static void LoadSettings()
        {
            string file = FileStorageManager.LoadSettingsFile();
            if (file != null)
            {
                string[] lines = file.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                // Split options key/value
                Dictionary<string, string> options = new Dictionary<string, string>();
                foreach (string line in lines)
                {
                    string[] keyValue = line.Split("=".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (keyValue.Length == 2)
                        options.Add(keyValue[0].Trim(), keyValue[1].Trim());
                }

                // Now we can use options dictionary to update settings
                if (options.ContainsKey("mute"))
                    IsMute = options["mute"] == "1";
                if (options.ContainsKey("volume"))
                    MasterVolume = float.Parse(options["volume"]);
                if (options.ContainsKey("fullscreen"))
                    IsFullscreen = options["fullscreen"] != "0";
                if (options.ContainsKey("bloom"))
                    BloomFX = options["bloom"] == "1";
                if (options.ContainsKey("sensitivity"))
                    Sensitivity = float.Parse(options["sensitivity"]);
#if ANDROID
                if (options.ContainsKey("orientationLock"))
                    _isOrientationLocked = options["orientationLock"] == "1";
                if (options.ContainsKey("orientation"))
                    ScreenOrientation = (DisplayOrientation) int.Parse(options["orientation"]);
#endif
                if (options.ContainsKey("sequentialMode"))
                    IsSequentialMode = options["sequentialMode"] == "1";
                if (options.ContainsKey("sequentialSeed"))
                    CurrentSequentialSeed = int.Parse(options["sequentialSeed"]);
            }
        }
    }
}
