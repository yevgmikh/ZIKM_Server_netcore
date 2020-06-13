using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;

namespace ZIKM.Server.Services.Storages.Factories {
    /// <summary>
    /// Database storage factory
    /// </summary>
    internal class DatabaseStorageFactory : IStorageFactory {
        public Storage GetStorage(PermissionLevel level, string user) {
            return new DatabaseStorage((uint)level, user);
        }
    }
}
