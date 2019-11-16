using System.Collections.Generic;

namespace ZIKM.Interfaces
{
    public interface IPermissionsLevel
    {
        /// <summary>
        /// Paths and levels of permissions
        /// </summary>
        Dictionary<string, int> Levels { get; }
    }
}
