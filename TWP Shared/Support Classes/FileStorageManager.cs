using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using TheWitnessPuzzles;

namespace TWP_Shared
{
    public static class FileStorageManager
    {
        private static readonly string SETTINGS_FILENAME = "settings.cfg";

        private static IsolatedStorageFile isolatedStorage;

        static FileStorageManager()
        {
#if WINDOWS
            isolatedStorage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
#endif    
        }

        public static string LoadSettingsFile()
        {
            // Open isolated storage and read settings file
            if (isolatedStorage.FileExists(SETTINGS_FILENAME))
            {
                using (IsolatedStorageFileStream fs = isolatedStorage.OpenFile(SETTINGS_FILENAME, System.IO.FileMode.Open))
                {
                    if (fs != null)
                    {
                        using (StreamReader reader = new StreamReader(fs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }

            return null;
        }

        public static void SaveSettingsFile(string file)
        {
            // Open isolated storage and write the settings file
            using (IsolatedStorageFileStream fs = isolatedStorage.OpenFile(SETTINGS_FILENAME, System.IO.FileMode.Create))
            {
                if (fs != null)
                {
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.Write(file);
                    }
                }
            }
        }

        public static string SerializePanel(Puzzle panel)
        {
            return null;
        }
    }
}
