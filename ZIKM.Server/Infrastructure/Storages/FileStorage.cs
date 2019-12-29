using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Infrastructure.Storages{
    class FileStorage : IStorage{
        private static readonly string _rootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Data");
        private static readonly IPermissionsLevel permissions = new PermissionData();
        private static readonly Dictionary<string, int> _permissions = new Dictionary<string, int>(permissions.Levels);

        /// <summary>
        /// User's permission level
        /// </summary>
        private readonly int _level;
        private string fileName;

        /// <summary>
        /// Current directory
        /// </summary>
        private string Path { get; set; } = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Data");
        /// <summary>
        /// Permission status of file(opened folder)
        /// </summary>
        private int Code { get; set; } = 0;

        public FileStorage(int level){
            _level = level;
            if (_permissions.ContainsValue(0)){
                if (!_permissions.ContainsKey(_rootPath))
                    throw new Exception("Incorrect root path in permission level dictionary");
            }
            else
                _permissions.Add(_rootPath, 0);
        }

        #region Helpers
        private ResponseData PermissionError(){
            Logger.ToLogAll("Not enough rights");
            return new ResponseData(2, "Not enough rights.");
        }
        #endregion

        #region Main operation
        public ResponseData GetAll(){
            var objects = Directory.GetDirectories(Path)
                .Select(i => $"folder:{new DirectoryInfo(i).Name}").ToList();
            objects.AddRange(Directory.GetFiles(Path)
                .Select(i => $"file:{new FileInfo(i).Name}"));
            return new ResponseData(0, string.Join(";", objects));
        }

        public ResponseData OpenFile(string name){
            // Check is not root folder
            if (Code != 0)
            {
                // Check file name
                if (Directory.GetFiles(Path).Select(i => new FileInfo(i).Name).Contains(name))
                {
                    fileName = name;
                    Logger.ToLog($"File {fileName} opened");
                    return new ResponseData(0, $"File {fileName} opened");
                }
                else
                    return new ResponseData(1, "File not found");
            }
            else
                return new ResponseData(1, "Here no files");
        }

        public ResponseData OpenFolder(string path){
            // Check is root folder
            if (Path == _rootPath){
                // Check name folder
                if (Directory.GetDirectories(Path).Select(i => new DirectoryInfo(i).Name).Contains(path)){
                    // Check permissions
                    int code = _permissions[System.IO.Path.Combine(Path, path)];
                    if (code >= _level - 1 && code <= _level + 1){
                        // Opening folder
                        Path = System.IO.Path.Combine(Path, path);
                        Code = code;
                        return new ResponseData(0, $"Folder {path} opened");
                    }
                    else
                        return new ResponseData(2, "Not enough rights.");
                }
                else
                    return new ResponseData(1, "Here no this folder");
            }
            else
                return new ResponseData(1, "Here no folders");
        }

        public ResponseData CloseFolder(){
            // Check is not root folder
            if (Path != _rootPath){
                // Closing folder
                Path = Directory.GetParent(Path).FullName;
                Code = _permissions[Path];
                return new ResponseData(0, "Folder closed");
            }
            else
                return new ResponseData(1, "You in home directory");
        }
        #endregion

        #region File operation

        public ResponseData ReadFile(){
            if (fileName == null)
                return new ResponseData(1, "File not opened");

            // Check permisions
            if ((Code + 1) == _level || Code == _level){
                try{
                    var texts = File.ReadAllText(System.IO.Path.Combine(Path, $"{fileName}"));
                    Logger.ToLog($"File {fileName} read");
                    return new ResponseData(0, $"{texts}");
                }
                catch (Exception e){
                    Logger.ToLogAll($"Error while {fileName} read, {e.Message}");
                    return new ResponseData(1, $"Error reading{e.Message}");
                }

            }
            else
                return PermissionError();
        }

        public ResponseData WriteToFile(string text){
            if (fileName == null)
                return new ResponseData(1, "File not opened");

            // Check permisions
            if ((Code - 1) == _level || Code == _level){
                try{
                    using (StreamWriter writer = new StreamWriter(System.IO.Path.Combine(Path, $"{fileName}"), true))
                        writer.WriteLine(text);

                    Logger.ToLog($"Saved to file {fileName}");
                    return new ResponseData(0, "Successfully");
                }
                catch (Exception e){
                    Logger.ToLogAll($"Error while saving file {fileName}, {e.Message}");
                    return new ResponseData(1, $"Error writing{e.Message}");
                }
            }
            else
                return PermissionError();
        }

        public ResponseData ChangeFile(string text)
        {
            if (fileName == null)
                return new ResponseData(1, "File not opened");

            if (Code == _level){
                // Check permisions
                try
                {
                    using (StreamWriter writer = new StreamWriter(System.IO.Path.Combine(Path, $"{fileName}"), false))
                        writer.WriteLine(text);

                    Logger.ToLog($"File {fileName} edited");
                    return new ResponseData(0, "Updated");
                }
                catch (Exception e){
                    Logger.ToLogAll($"Error while editing file {fileName}, {e.Message}");
                    return new ResponseData(1, $"Error editing:{e.Message}");
                }
            }
            else
                return PermissionError();
        }

        public ResponseData CloseFile(){
            if (fileName == null)
                return new ResponseData(1, "File not opened");

            string name = fileName;
            fileName = null;
            Logger.ToLog($"File {name} closed");
            return new ResponseData(0, $"File {name} closed");
        }

        #endregion
    }
}
