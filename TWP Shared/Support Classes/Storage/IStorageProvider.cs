using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TWP_Shared
{
    public interface IStorageProvider
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        void CreateDirectory(string path);

        FileStream OpenFile(string path, FileMode mode);
        void MoveFile(string sourcePath, string destinationPath);
        void DeleteFile(string path);

        string[] GetFileNames(string searchPattern);
    }
}
