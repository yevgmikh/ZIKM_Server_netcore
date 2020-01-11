using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Infrastructure.Storages.Model;
using ZIKM.Server.Infrastructure;

namespace ZIKM.Infrastructure.Storages {
    class DatabaseStorage : IStorage, IAuthorization {
        /// <summary>
        /// User's permission level
        /// </summary>
        private readonly int _level;
        protected string fileName;
        protected int folderId = 1;
        protected StorageContext _db = new StorageContext();

        public DatabaseStorage(int level = -1) {
            _level = level;
        }

        #region Helpers
        private ResponseData PermissionError() {
            Logger.ToLogAll("Not enough rights");
            return new ResponseData(StatusCode.NoAccess, Messages.NoAccess);
        }
        #endregion

        #region File
        public ResponseData ReadFile() {
            var file = _db.FolderTrees.FirstOrDefault(file => file.Name == fileName)?.File;

            if (file == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            if ((file.Permission + 1) == _level || file.Permission == _level) {
                try {
                    Logger.ToLog($"File {fileName} read");
                    return new ResponseData(StatusCode.Success, Encoding.UTF8.GetString(file.Data));
                }
                catch (Exception ex) {
                    Logger.ToLogAll($"Error while reading file {fileName}, {ex.Message}");
                    return new ResponseData(StatusCode.ServerError, Messages.ReadError(ex));
                }
            }
            else
                return PermissionError();

        }

        public ResponseData WriteToFile(string text) {
            var file = _db.FolderTrees.FirstOrDefault(file => file.Name == fileName)?.File;

            if (file == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            if ((file.Permission - 1) == _level || file.Permission == _level) {
                try {
                    byte[] textBytes = Encoding.UTF8.GetBytes(text);
                    byte[] data = new byte[file.Data.Length + textBytes.Length];
                    Buffer.BlockCopy(file.Data, 0, data, 0, file.Data.Length);
                    Buffer.BlockCopy(textBytes, 0, data, file.Data.Length, textBytes.Length);
                    file.Data = data;
                    _db.SaveChanges();

                    Logger.ToLog($"Saved to file {fileName}");
                    return new ResponseData(StatusCode.Success, Messages.Written);
                }
                catch (Exception ex) {
                    Logger.ToLogAll($"Error while saving file {fileName}, {ex.Message}");
                    return new ResponseData(StatusCode.ServerError, Messages.WriteError(ex));
                }
            }
            else
                return PermissionError();
        }

        public ResponseData ChangeFile(string text) {
            var file = _db.FolderTrees.FirstOrDefault(file => file.Name == fileName)?.File;

            if (file == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            if (file.Permission == _level) {
                try {
                    file.Data = Encoding.UTF8.GetBytes(text);
                    _db.SaveChanges();

                    Logger.ToLog($"File {fileName} edited");
                    return new ResponseData(StatusCode.Success, Messages.Updated);
                }
                catch (Exception ex)
                {
                    Logger.ToLogAll($"Error while editing file {fileName}, {ex.Message}");
                    return new ResponseData(StatusCode.ServerError, Messages.EditError(ex));
                }
            }
            else
                return PermissionError();
        }

        public ResponseData CloseFile() {
            if (fileName == null)
                return new ResponseData(StatusCode.ServerError, Messages.NotOpened);

            string name = fileName;
            fileName = null;
            Logger.ToLog($"File {name} closed");
            return new ResponseData(StatusCode.Success, Messages.FileClosed(name));
        }
        #endregion

        #region Main
        public ResponseData GetAll() {
            return new ResponseData(StatusCode.Success, string.Join(";", _db.FolderTrees.Include(t => t.ParentFolder)
                .Where(obj => obj.ParentFolder.Id == folderId)
                .Include(t => t.Folder)
                .Select(obj => $"{(obj.Folder != null ? "folder" : "file")}:{obj.Name}")));
        }

        public ResponseData OpenFile(string name) {
            var file = _db.FolderTrees.Include(t => t.ParentFolder).Include(t => t.File)
                .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId).File;
            if (file == null)
                return new ResponseData(StatusCode.BadData, Messages.FileNotFound(name));

            if (file.Permission >= _level - 1 && file.Permission <= _level + 1) {
                fileName = name;
                return new ResponseData(StatusCode.Success, Messages.FileOpened(name));
            }
            return PermissionError();
        }

        public ResponseData OpenFolder(string name) {
            var folder = _db.FolderTrees.Include(t => t.ParentFolder).Include(t => t.Folder)
                .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId).Folder;
            if (folder == null)
                return new ResponseData(StatusCode.BadData, Messages.FolderNotFound(name));

            if (folder.Permission >= _level - 1 && folder.Permission <= _level + 1) {
                folderId = folder.Id;
                return new ResponseData(StatusCode.Success, Messages.FolderOpened(name));
            }
            return PermissionError();
        }

        public ResponseData CloseFolder() {
            if (folderId == 1)
                return new ResponseData(StatusCode.BadData, Messages.RootFolder);

            folderId = _db.FolderTrees.First(obj => obj.FolderId == folderId).ParentFolderId.Value;
            return new ResponseData(StatusCode.Success, Messages.FolderClosed);
        }
        #endregion

        public ResponseData SingIn(string login, string password) {
            var user = _db.Users.Include(obj => obj.Passwords).FirstOrDefault(u => u.Name == login);

            if (user == null) {
                // No user in data
                Logger.ToLogAll($"{login} not found");
                return new ResponseData(StatusCode.BadData, Messages.NotFound(login));
            }

            var currentPassword = user.Passwords.FirstOrDefault(p => !p.IsUsed);
            if (currentPassword == null) {
                #region User's spent all passwords
                switch (login){
                    case "Master":
                        Logger.ToLogAll("Fake master");
                        return new ResponseData(StatusCode.Blocked, Messages.MasterBlocked);
                    case "Senpai":
                        Logger.ToLogAll("Impostor");
                        return new ResponseData(StatusCode.Blocked, Messages.SempaiBlocked);
                    case "Kouhai":
                        Logger.ToLogAll("Liar");
                        return new ResponseData(StatusCode.Blocked, Messages.KouhaiBlocked);
                    default:
                        Logger.ToLogAll($"{login} blocked");
                        return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                }
                #endregion
            }
            currentPassword.IsUsed = true;
            _db.SaveChanges();

            if (currentPassword.Password == password) {
                return new ResponseData();
            }
            else {
                if (user.Passwords.Count(p => !p.IsUsed) == 0) {
                    // User's spent last password 
                    Logger.ToLogAll($"{login} blocked");
                    return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                }
                else {
                    // User's written wrong password
                    Logger.ToLogAll($"{login} errored");
                    return new ResponseData(StatusCode.BadData, Messages.TryAgain);
                }
            }
        }
    }
}
