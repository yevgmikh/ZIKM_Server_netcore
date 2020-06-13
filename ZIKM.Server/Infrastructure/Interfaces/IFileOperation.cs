using ZIKM.Infrastructure.DataStructures;

namespace ZIKM.Server.Infrastructure.Interfaces {
    /// <summary>
    /// File operations
    /// </summary>
    internal interface IFileOperation {
        /// <summary>
        /// Read from current opened file
        /// </summary>
        /// <returns>Status and data of operation</returns>
        ResponseData ReadFile();
        /// <summary>
        /// Write to current opened file
        /// </summary>
        /// <param name="text">New text</param>
        /// <returns>Status and data of operation</returns>
        ResponseData WriteToFile(string text);
        /// <summary>
        /// Change current opened file
        /// </summary>
        /// <param name="text">Edited text</param>
        /// <returns>Status and data of operation</returns>
        ResponseData ChangeFile(string text);
        /// <summary>
        /// Close current opened file
        /// </summary>
        /// <returns>Status and data of operation</returns>
        ResponseData CloseFile();
        /// <summary>
        /// Locks file for editing by another user
        /// </summary>
        /// <returns>Status of operation</returns>
        bool LockFile();
        /// <summary>
        /// Unlocks file for editing by another user
        /// </summary>
        void UnlockFile();
    }
}
