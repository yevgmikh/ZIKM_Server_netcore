using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;

namespace ZIKM.Server.Services.Storages.Factories {
    /// <summary>
    /// File storage factory
    /// </summary>
    internal class FileStorageFactory : IStorageFactory {
        public Storage GetStorage(PermissionLevel level, string user) {
            return new FileStorage((uint)level, user);
        }
    }
}
