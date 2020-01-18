using ZIKM.Infrastructure;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Services.Storages.Factories {
    /// <summary>
    /// Database storage factory
    /// </summary>
    internal class DatabaseStorageFactory : IStorageFactory {
        public IStorage GetStorage(PermissionLevel level, string user) {
            return new DatabaseStorage((uint)level, user);
        }
    }
}
