using System.Collections.Generic;

namespace ZIKM.Server.Infrastructure.Interfaces{
    public interface IPermissionsLevel{
        /// <summary>
        /// Paths and levels of permissions
        /// </summary>
        Dictionary<string, int> Levels { get; }
    }
}
