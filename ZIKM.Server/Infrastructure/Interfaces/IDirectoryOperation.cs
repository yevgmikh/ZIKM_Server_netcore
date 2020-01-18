using ZIKM.Infrastructure.DataStructures;

namespace ZIKM.Infrastructure.Interfaces {
    /// <summary>
    /// Directory operations
    /// </summary>
    internal interface IDirectoryOperation {
        /// <summary>
        /// Get all folders and files of current directory
        /// </summary>
        /// <returns>Status and data of operation</returns>
        ResponseData GetAll();
        /// <summary>
        /// Open folder
        /// </summary>
        /// <param name="name">Folder name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData OpenFile(string name);
        /// <summary>
        /// Open file
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData OpenFolder(string name);
        /// <summary>
        /// Close current opened folder
        /// </summary>
        /// <returns>Status and data of operation</returns>
        ResponseData CloseFolder();
    }
}
