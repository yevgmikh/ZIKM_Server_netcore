using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using ZIKM.Infrastructure;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Services.Storages.Model;

namespace ZIKM.Services.Storages {
    /// <summary>
    /// Database storage of data
    /// </summary>
    internal class DatabaseStorage : IStorage {
        /// <summary>
        /// User's permission level
        /// </summary>
        private readonly uint _level;
        private readonly string _user;
        protected string fileName;
        protected int folderId = 1;
        protected StorageContext _db = IoC.GetService<StorageContext>();

        public DatabaseStorage(uint level, string user) {
            _level = level;
            _user = user;
        }

        #region Helpers
        private ResponseData PermissionError() {
            Logger.ToLogAll(LogMessages.NoAccess(_user));
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
                    Logger.ToLog(LogMessages.FileRead(_user, fileName));
                    return new ResponseData(StatusCode.Success, Encoding.UTF8.GetString(file.Data));
                }
                catch (Exception ex) {
                    Logger.ToLogAll(LogMessages.ReadError(_user, fileName, ex));
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

                    Logger.ToLog(LogMessages.Written(_user, fileName));
                    return new ResponseData(StatusCode.Success, Messages.Written);
                }
                catch (Exception ex) {
                    Logger.ToLogAll(LogMessages.WriteError(_user, fileName, ex));
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

                    Logger.ToLog(LogMessages.Updated(_user, fileName));
                    return new ResponseData(StatusCode.Success, Messages.Updated);
                }
                catch (Exception ex)
                {
                    Logger.ToLogAll(LogMessages.EditError(_user, fileName, ex));
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
            Logger.ToLog(LogMessages.FileClosed(_user, name));
            return new ResponseData(StatusCode.Success, Messages.FileClosed(name));
        }
        #endregion

        #region Main
        public ResponseData GetAll() {
            return new ResponseData(StatusCode.Success, string.Join(";", _db.FolderTrees.Include(t => t.ParentFolder)
                .Where(obj => obj.ParentFolder.Id == folderId)
                .Include(t => t.Folder).Where(obj => obj.FolderId != folderId)
                .Select(obj => $"{(obj.Folder != null ? "folder" : "file")}:{obj.Name}")));
        }

        public ResponseData OpenFile(string name) {
            var file = _db.FolderTrees.Include(t => t.ParentFolder).Include(t => t.File)
                .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId).File;
            if (file == null)
                return new ResponseData(StatusCode.BadData, Messages.FileNotFound(name));

            if (file.Permission >= _level - 1 && file.Permission <= _level + 1) {
                fileName = name;
                Logger.ToLog(LogMessages.FileOpened(_user, fileName));
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
                Logger.ToLog(LogMessages.FolderOpened(_user, name));
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
    }
}
