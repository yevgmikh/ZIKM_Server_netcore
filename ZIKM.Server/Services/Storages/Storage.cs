using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Services.Storages {
    /// <summary>
    /// Storage of data
    /// </summary>
    internal abstract class Storage : IFileOperation, IDirectoryOperation {
        /// <summary>
        /// User's permission level
        /// </summary>
        protected readonly uint _level;
        protected readonly string _user;

        protected Storage(uint level, string user) {
            _level = level;
            _user = user;
        }

        #region  Helpers

        protected ResponseData PermissionError() {
            Logger.LogAll(LogMessages.NoAccess(_user));
            return new ResponseData(StatusCode.NoAccess, Messages.NoAccess);
        }

        protected bool CkeckOpenPermission(uint level) {
            return level < _level - 1 || level > _level + 1;
        }

        protected bool CkeckReadPermission(uint level) {
            return (level + 1) != _level && level != _level;
        }

        protected bool CkeckWritePermission(uint level) {
            return (level - 1) != _level && level != _level;
        }

        protected bool CkeckEditPermission(uint? level) {
            return level != _level;
        }

        #endregion

        #region Abstract methods
        public abstract ResponseData AddFile(string name);
        public abstract ResponseData AddFolder(string name);
        public abstract ResponseData ChangeFile(string text);
        public abstract ResponseData CloseFile();
        public abstract ResponseData CloseFolder();
        public abstract ResponseData EditFile(string name, string newName);
        public abstract ResponseData EditFolder(string name, string newName);
        public abstract ResponseData GetAll();
        public abstract bool LockFile();
        public abstract ResponseData OpenFile(string name);
        public abstract ResponseData OpenFolder(string name);
        public abstract ResponseData ReadFile();
        public abstract ResponseData RemoveFile(string name);
        public abstract ResponseData RemoveFolder(string name);
        public abstract void UnlockFile();
        public abstract ResponseData WriteToFile(string text);

        #endregion
    }
}
