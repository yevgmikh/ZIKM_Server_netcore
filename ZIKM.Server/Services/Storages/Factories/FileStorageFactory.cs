using ZIKM.Infrastructure;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Services.Storages.Factories {
    /// <summary>
    /// File storage factory
    /// </summary>
    internal class FileStorageFactory : IStorageFactory {
        public IStorage GetStorage(PermissionLevel level, string user) {
            return new FileStorage((uint)level, user);
        }
    }
}
