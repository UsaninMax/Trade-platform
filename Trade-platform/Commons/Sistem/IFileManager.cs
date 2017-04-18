﻿namespace TradePlatform.Commons.Sistem
{
    public interface IFileManager
    {
        void OpenFolder(string path);
        void DeleteFolder(string path);
        void CreateFolder(string path);
        bool IsFileExist(string path);
        bool IsDirectoryExist(string path);
    }
}