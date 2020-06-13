﻿using ZIKM.Server.Services.Storages;

namespace ZIKM.Server.Infrastructure.Interfaces {
    /// <summary>
    /// Storage factory
    /// </summary>
    internal interface IStorageFactory {
        /// <summary>
        /// Get storage of data
        /// </summary>
        /// <param name="level">Permission level</param>
        /// <param name="user">User name</param>
        /// <returns>Storage of data</returns>
        Storage GetStorage(PermissionLevel level, string user);
    }
}
