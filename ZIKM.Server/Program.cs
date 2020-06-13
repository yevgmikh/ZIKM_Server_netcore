using System;
using ZIKM.Server.Servers;
using ZIKM.Server.Servers.Tcp;
using ZIKM.Server.Utils;

namespace ZIKM.Server {
    class Program {
        static void Main(string[] args) {
            try {
                ServerObject server = new TCPServer();
                server.Start();
            }
            catch(Exception ex) {
                Logger.LogCritical(ex.Message);
                Logger.LogCritical(ex.StackTrace);
                while (ex.InnerException != null) {
                    Logger.LogCritical(ex.InnerException.Message);
                    Logger.LogCritical(ex.InnerException.StackTrace);
                    ex = ex.InnerException;
                }
            }
        }
    }
}
