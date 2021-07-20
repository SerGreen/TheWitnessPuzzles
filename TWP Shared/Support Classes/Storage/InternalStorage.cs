using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

namespace TWP_Shared
{
    public class InternalStorage : IStorageProvider
    {
        private readonly IsolatedStorageFile storage;

        public InternalStorage()
        {
#if WINDOWS
            storage = IsolatedStorageFile.GetUserStoreForDomain();
#else
            storage = IsolatedStorageFile.GetUserStoreForApplication();
#endif
        }

        public bool FileExists(string path) => storage.FileExists(path);
        public bool DirectoryExists(string path) => storage.DirectoryExists(path);
        public void CreateDirectory(string path) => storage.CreateDirectory(path);

        public FileStream OpenFile(string path, FileMode mode) => storage.OpenFile(path, mode);
        public void MoveFile(string sourcePath, string destinationPath) => storage.MoveFile(sourcePath, destinationPath);
        public void DeleteFile(string path) => storage.DeleteFile(path);

        public string[] GetFileNames(string searchPattern)
        {
            string[] files = storage.GetFileNames(searchPattern);
            Array.Sort<string>(files, (a, b) => storage.GetLastWriteTime(b).CompareTo(storage.GetLastWriteTime(a)));
            return files;
        }
    }
}
