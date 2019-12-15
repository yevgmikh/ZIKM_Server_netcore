using System.Collections.Generic;
using System.IO;
using ZIKM.Interfaces;

namespace ZIKM{
    class PermissionData : IPermissionsLevel{
        public Dictionary<string, int> Levels { get; private set; } = new Dictionary<string, int>(){
            { Path.Combine(Directory.GetCurrentDirectory(), "Data", "1"), 1 },
            { Path.Combine(Directory.GetCurrentDirectory(), "Data", "2"), 2 },
            { Path.Combine(Directory.GetCurrentDirectory(), "Data", "3"), 3 },
            { Path.Combine(Directory.GetCurrentDirectory(), "Data", "4"), 4 }
        };
    }
}
