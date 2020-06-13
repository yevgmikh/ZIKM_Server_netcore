using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Services.Storages.Model;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Services.Storages {
    /// <summary>
    /// Database storage of data
    /// </summary>
    internal class DatabaseStorage : Storage {
        /// <summary>
        /// Locked files for editing by another user
        /// </summary>
        /// <typeparam name="int">File ID</typeparam>
        /// <returns></returns>
        private static readonly HashSet<int> _lockedFiles = new HashSet<int>();

        /// <summary>
        /// Locked files for append text by another user
        /// </summary>
        /// <typeparam name="int">File ID</typeparam>
        /// <returns></returns>
        private static readonly HashSet<int> _lockedWrite = new HashSet<int>();
        
        protected int fileId;
        protected string fileName;
        protected int folderId = 1;
        protected StorageContext _db = IoC.GetService<StorageContext>();

        public DatabaseStorage(uint level, string user) : base(level, user) { }

        #region Helpers

        private void CheckFileWritting() {
            while (_lockedWrite.Contains(fileId))
                Thread.Sleep(100);
            _lockedWrite.Add(fileId);
        }

        #endregion

        #region File

        public override ResponseData ReadFile() {
            var file = _db.Files.Find(fileId);

            if (file == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            if (CkeckReadPermission(file.Permission))
                return PermissionError();

            try {
                Logger.Log(LogMessages.FileRead(_user, fileName));
                return new ResponseData(StatusCode.Success, Encoding.UTF8.GetString(file.Data));
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.ReadError(_user, fileName, ex));
                return new ResponseData(StatusCode.ServerError, Messages.ReadError);
            }
        }

        public override ResponseData WriteToFile(string text) {
            var file = _db.Files.Find(fileId);

            if (file == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            if (CkeckWritePermission(file.Permission))
                return PermissionError();

            try {
                if (_lockedFiles.Contains(fileId))
                    return new ResponseData(StatusCode.NoAccess, Messages.FileLocked);
                CheckFileWritting();

                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                byte[] data = new byte[file.Data.Length + textBytes.Length];
                Buffer.BlockCopy(file.Data, 0, data, 0, file.Data.Length);
                Buffer.BlockCopy(textBytes, 0, data, file.Data.Length, textBytes.Length);
                file.Data = data;
                _db.SaveChanges();
                _lockedWrite.Remove(fileId);

                Logger.Log(LogMessages.Written(_user, fileName));
                return new ResponseData(StatusCode.Success, Messages.Written);
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.WriteError(_user, fileName, ex));
                return new ResponseData(StatusCode.ServerError, Messages.WriteError);
            }
        }

        public override ResponseData ChangeFile(string text) {
            var file = _db.Files.Find(fileId);

            if (file == null)
                return new ResponseData(StatusCode.ServerError, Messages.NoFile);

            if (CkeckEditPermission(file.Permission))
                return PermissionError();

            try {
                file.Data = Encoding.UTF8.GetBytes(text);
                _db.SaveChanges();
                _lockedFiles.Remove(fileId);

                Logger.Log(LogMessages.Updated(_user, fileName));
                return new ResponseData(StatusCode.Success, Messages.Updated);
            }
            catch (Exception ex)
            {
                Logger.LogError(LogMessages.EditError(_user, fileName, ex));
                return new ResponseData(StatusCode.ServerError, Messages.EditError);
            }
        }

        public override ResponseData CloseFile() {
            if (fileId == 0)
                return new ResponseData(StatusCode.ServerError, Messages.NotOpened);

            fileId = 0;
            string name = fileName;
            fileName = null;
            Logger.Log(LogMessages.FileClosed(_user, name));
            return new ResponseData(StatusCode.Success, Messages.FileClosed(name));
        }

        public override bool LockFile() {
            if (_lockedFiles.Contains(fileId))
                return false;
            else{
                _lockedFiles.Add(fileId);
                return true;
            }
        }

        public override void UnlockFile() {
            _lockedFiles.Remove(fileId);
        }

        #endregion

        #region Main

        public override ResponseData GetAll() {
            return new ResponseData(StatusCode.Success, string.Join(";", _db.FolderTrees
                .Where(obj => obj.ParentFolder.Id == folderId)
                .Where(obj => obj.FolderId != folderId)
                .Select(obj => $"{(obj.Folder != null ? "folder" : "file")}:{obj.Name}")));
        }

        public override ResponseData OpenFile(string name) {
            // Getting the file
            var file = _db.FolderTrees
                .Include(t => t.File)
                .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId).File;

            if (file == null)
                return new ResponseData(StatusCode.BadData, Messages.FileNotFound(name));

            if (CkeckOpenPermission(file.Permission))
                return PermissionError();
            
            // Marking file openning
            fileId = file.Id;
            fileName = name;
            Logger.Log(LogMessages.FileOpened(_user, fileName));
            return new ResponseData(StatusCode.Success, Messages.FileOpened(name));
        }

        public override ResponseData OpenFolder(string name) {
            // Getting the folder
            var folder = _db.FolderTrees
                .Include(t => t.Folder)
                .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId).Folder;

            if (folder == null)
                return new ResponseData(StatusCode.BadData, Messages.FolderNotFound(name));

            if (CkeckOpenPermission(folder.Permission))
                return PermissionError();
            
            // Marking folder openning
            folderId = folder.Id;
            Logger.Log(LogMessages.FolderOpened(_user, name));
            return new ResponseData(StatusCode.Success, Messages.FolderOpened(name));
        }

        public override ResponseData CloseFolder() {
            // Check is root folder
            if (folderId == 1)
                return new ResponseData(StatusCode.BadData, Messages.RootFolder);

            // Marking folder closing
            folderId = _db.FolderTrees.First(obj => obj.FolderId == folderId).ParentFolderId.Value;
            return new ResponseData(StatusCode.Success, Messages.FolderClosed);
        }

        public override ResponseData AddFile(string name) {
            var folder = _db.Folders.Find(folderId);
            if (CkeckEditPermission(folder?.Permission))
                return PermissionError();

            try {
                var file = new DataFile() {
                    Permission = _level,
                    Data = new byte[0]
                };

                _db.Files.Add(file);
                _db.FolderTrees.Add(new FolderTree(name, folder, file));
                _db.SaveChanges();

                Logger.Log(LogMessages.FileAdded(_user, name));
                return new ResponseData(StatusCode.Success, Messages.FileAdded(name));
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.AddFileError(_user, name, ex));
                return new ResponseData(StatusCode.ServerError, Messages.AddFileError);
            }
        }

        public override ResponseData EditFile(string name, string newName) {
            var folder = _db.Folders.Find(folderId);
            if (CkeckEditPermission(folder?.Permission))
                return PermissionError();

            try {
                var folderTree = _db.FolderTrees
                    .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId);
                if (_lockedFiles.Contains(folderTree.FileId.Value))
                    return new ResponseData(StatusCode.NoAccess, Messages.FileLocked);
                
                _db.FolderTrees.Remove(folderTree);
                _db.FolderTrees.Add(new FolderTree(newName, folderTree.ParentFolder, folderTree.File));
                _db.SaveChanges();

                Logger.Log(LogMessages.FileEdited(_user, name, newName));
                return new ResponseData(StatusCode.Success, Messages.FileEdited(name));
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.EditFileError(_user, name, ex));
                return new ResponseData(StatusCode.ServerError, Messages.EditFileError);
            }
        }

        public override ResponseData RemoveFile(string name) {
            var folder = _db.Folders.Find(folderId);
            if (CkeckEditPermission(folder?.Permission))
                return PermissionError();

            try {
                var folderTree = _db.FolderTrees
                    .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId);
                if (_lockedFiles.Contains(folderTree.FileId.Value))
                    return new ResponseData(StatusCode.NoAccess, Messages.FileLocked);
                
                _db.FolderTrees.Remove(folderTree);
                _db.Files.Remove(folderTree.File);
                _db.SaveChanges();

                Logger.Log(LogMessages.FileRemoved(_user, name));
                return new ResponseData(StatusCode.Success, Messages.FileRemoved(name));
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.RemoveFileError(_user, name, ex));
                return new ResponseData(StatusCode.ServerError, Messages.RemoveFileError);
            }
        }

        public override ResponseData AddFolder(string name) {
            var folder = _db.Folders.Find(folderId);
            if (CkeckEditPermission(folder?.Permission))
                return PermissionError();

            try {
                var newFolder = new Folder() {
                    Permission = _level
                };

                _db.Folders.Add(newFolder);
                _db.FolderTrees.Add(new FolderTree(name, folder, newFolder));
                _db.SaveChanges();

                Logger.Log(LogMessages.FolderAdded(_user, name));
                return new ResponseData(StatusCode.Success, Messages.FolderAdded(name));
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.AddFolderError(_user, name, ex));
                return new ResponseData(StatusCode.ServerError, Messages.AddFolderError);
            }
        }

        public override ResponseData EditFolder(string name, string newName) {
            var folder = _db.Folders.Find(folderId);
            if (CkeckEditPermission(folder?.Permission))
                return PermissionError();

            try {
                var folderTree = _db.FolderTrees
                    .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId);
                
                _db.FolderTrees.Remove(folderTree);
                _db.FolderTrees.Add(new FolderTree(newName, folderTree.ParentFolder, folderTree.Folder));
                _db.SaveChanges();

                Logger.Log(LogMessages.FolderEdited(_user, name, newName));
                return new ResponseData(StatusCode.Success, Messages.FolderEdited(name));
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.EditFolderError(_user, name, ex));
                return new ResponseData(StatusCode.ServerError, Messages.EditFolderError);
            }
        }

        public override ResponseData RemoveFolder(string name) {
            var folder = _db.Folders.Find(folderId);
            if (CkeckEditPermission(folder?.Permission))
                return PermissionError();

            try {
                var folderTree = _db.FolderTrees
                    .FirstOrDefault(fol => fol.Name == name && fol.ParentFolderId == folderId);
                //if (_lockedFiles.Contains(folderTree.FileId.Value))
                    //return new ResponseData(StatusCode.NoAccess, Messages.FileLocked);
                
                _db.FolderTrees.Remove(folderTree);
                _db.Folders.Remove(folderTree.Folder);
                _db.SaveChanges();

                Logger.Log(LogMessages.FolderRemoved(_user, name));
                return new ResponseData(StatusCode.Success, Messages.FolderRemoved(name));
            }
            catch (Exception ex) {
                Logger.LogError(LogMessages.RemoveFolderError(_user, name, ex));
                return new ResponseData(StatusCode.ServerError, Messages.RemoveFolderError);
            }
        }

        #endregion
    }
}
