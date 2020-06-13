using ZIKM.Infrastructure.DataStructures;

namespace ZIKM.Server.Infrastructure.Interfaces {
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

        /// <summary>
        /// Add new file in current folder
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData AddFile(string name);

        /// <summary>
        /// Edit file in current folder
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="newName">New file name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData EditFile(string name, string newName);

        /// <summary>
        /// Remove file in current folder
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData RemoveFile(string name);

        /// <summary>
        /// Add new folder in current folder
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData AddFolder(string name);

        /// <summary>
        /// Edit folder in current folder
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="newName">New file name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData EditFolder(string name, string newName);

        /// <summary>
        /// Remove folder in current folder
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Status and data of operation</returns>
        ResponseData RemoveFolder(string name);
    }
}
