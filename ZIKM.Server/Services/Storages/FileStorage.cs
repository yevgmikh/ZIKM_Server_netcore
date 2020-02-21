using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;
using static System.IO.Path;

namespace ZIKM.Server.Services.Storages{
    /// <summary>
    /// File storage of data
    /// </summary>
    class FileStorage : IStorage {
        private static readonly string _rootPath = Combine(Directory.GetCurrentDirectory(), "Data");
        private static readonly Dictionary<string, int> _permissions = new Dictionary<string, int>() {
            { Combine(Directory.GetCurrentDirectory(), "Data", "1"), 1 },
            { Combine(Directory.GetCurrentDirectory(), "Data", "2"), 2 },
            { Combine(Directory.GetCurrentDirectory(), "Data", "3"), 3 },
            { Combine(Directory.GetCurrentDirectory(), "Data", "4"), 4 }
        };

        static FileStorage() {
            // Adding default files
            Directory.CreateDirectory(Combine(_rootPath, "1"));
            Directory.CreateDirectory(Combine(_rootPath, "2"));
            Directory.CreateDirectory(Combine(_rootPath, "3"));
            Directory.CreateDirectory(Combine(_rootPath, "4"));
            
            if (!File.Exists(Combine(_rootPath, "1", "file.txt")))
                using (StreamWriter writer = new StreamWriter(Combine(_rootPath, "1", "file.txt"), true))
                    writer.WriteLine("some text");
            if (!File.Exists(Combine(_rootPath, "2", "some.txt")))
                using (StreamWriter writer = new StreamWriter(Combine(_rootPath, "2", "some.txt"), true))
                    writer.WriteLine("some thing");
            if (!File.Exists(Combine(_rootPath, "3", "Plans.txt")))
                using (StreamWriter writer = new StreamWriter(Combine(_rootPath, "3", "Plans.txt"), true))
                    writer.WriteLine("some plans");
            if (!File.Exists(Combine(_rootPath, "4", "Reports.txt")))
                using (StreamWriter writer = new StreamWriter(Combine(_rootPath, "4", "Reports.txt"), true))
                    writer.WriteLine("some reports");
        }

        /// <summary>
        /// User's permission level
        /// </summary>
        private readonly uint _level;
        private readonly string _user;
        private string fileName;

        /// <summary>
        /// Current directory
        /// </summary>
        private string Path { get; set; } = Combine(Directory.GetCurrentDirectory(), "Data");
        /// <summary>
        /// Permission status of file(opened folder)
        /// </summary>
        private int Code { get; set; } = 0;

        public FileStorage(uint level, string user) {
            _user = user;
            _level = level;
            if (_permissions.ContainsValue(0)){
                if (!_permissions.ContainsKey(_rootPath))
                    throw new Exception("Incorrect root path in permission level dictionary");
            }
            else
                _permissions.Add(_rootPath, 0);
        }

        #region Helpers
        private ResponseData PermissionError() {
            Logger.LogAll(LogMessages.NoAccess(_user));
            return new ResponseData(StatusCode.NoAccess, Messages.NoAccess);
        }
        #endregion

        #region Main operation
        public ResponseData GetAll(){
            var objects = Directory.GetDirectories(Path)
                .Select(i => $"folder:{new DirectoryInfo(i).Name}").ToList();
            objects.AddRange(Directory.GetFiles(Path)
                .Select(i => $"file:{new FileInfo(i).Name}"));
            return new ResponseData(StatusCode.Success, string.Join(";", objects));
        }

        public ResponseData OpenFile(string name){
            // Check is not root folder
            if (Code != 0)
            {
                // Check file name
                if (Directory.GetFiles(Path).Select(i => new FileInfo(i).Name).Contains(name))
                {
                    fileName = name;
                    Logger.Log(LogMessages.FileOpened(_user, fileName));
                    return new ResponseData(StatusCode.Success, Messages.FileOpened(fileName));
                }
                else
                    return new ResponseData(StatusCode.BadData, Messages.FileNotFound(fileName));
            }
            else
                return new ResponseData(StatusCode.BadData, "Here no files");
        }

        public ResponseData OpenFolder(string path){
            // Check is root folder
            if (Path == _rootPath){
                // Check name folder
                if (Directory.GetDirectories(Path).Select(i => new DirectoryInfo(i).Name).Contains(path)){
                    // Check permissions
                    int code = _permissions[Combine(Path, path)];
                    if (code >= _level - 1 && code <= _level + 1){
                        // Opening folder
                        Path = Combine(Path, path);
                        Code = code;
                        return new ResponseData(StatusCode.Success, Messages.FolderOpened(path));
                    }
                    else
                        return PermissionError();
                }
                else
                    return new ResponseData(StatusCode.BadData, Messages.FolderNotFound(path));
            }
            else
                return new ResponseData(StatusCode.BadData, "Here no folders");
        }

        public ResponseData CloseFolder(){
            // Check is not root folder
            if (Path != _rootPath){
                // Closing folder
                Path = Directory.GetParent(Path).FullName;
                Code = _permissions[Path];
                return new ResponseData(StatusCode.Success, Messages.FolderClosed);
            }
            else
                return new ResponseData(StatusCode.BadData, Messages.RootFolder);
        }
        #endregion

        #region File operation

        public ResponseData ReadFile(){
            if (fileName == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            // Check permisions
            if ((Code + 1) == _level || Code == _level){
                try{
                    var texts = File.ReadAllText(Combine(Path, $"{fileName}"));
                    Logger.Log(LogMessages.FileRead(_user, fileName));
                    return new ResponseData(StatusCode.Success, $"{texts}");
                }
                catch (Exception e){
                    Logger.LogError(LogMessages.ReadError(_user, fileName, e));
                    return new ResponseData(StatusCode.ServerError, Messages.ReadError);
                }

            }
            else
                return PermissionError();
        }

        public ResponseData WriteToFile(string text){
            if (fileName == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            // Check permisions
            if ((Code - 1) == _level || Code == _level){
                try{
                    using (StreamWriter writer = new StreamWriter(Combine(Path, $"{fileName}"), true))
                        writer.WriteLine(text);

                    Logger.Log(LogMessages.Written(_user, fileName));
                    return new ResponseData(StatusCode.Success, Messages.Written);
                }
                catch (Exception e){
                    Logger.LogError(LogMessages.WriteError(_user, fileName, e));
                    return new ResponseData(StatusCode.ServerError, Messages.WriteError);
                }
            }
            else
                return PermissionError();
        }

        public ResponseData ChangeFile(string text)
        {
            if (fileName == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            if (Code == _level){
                // Check permisions
                try
                {
                    using (StreamWriter writer = new StreamWriter(Combine(Path, $"{fileName}"), false))
                        writer.WriteLine(text);

                    Logger.Log(LogMessages.Updated(_user, fileName));
                    return new ResponseData(StatusCode.Success, Messages.Updated);
                }
                catch (Exception e){
                    Logger.LogError(LogMessages.EditError(_user, fileName, e));
                    return new ResponseData(StatusCode.ServerError, Messages.EditError);
                }
            }
            else
                return PermissionError();
        }

        public ResponseData CloseFile(){
            if (fileName == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            string name = fileName;
            fileName = null;
            Logger.Log(LogMessages.FileClosed(_user, name));
            return new ResponseData(StatusCode.Success, Messages.FileClosed(name));
        }

        #endregion
    }
}
