using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZIKM.Server.Servers.Providers;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Servers {
    class TCPServer : ServerObject {
        public override void Start() {
            TcpListener server=null;
            try {
                server = new TcpListener(GetLocalIPAddress(), 8000);
 
                server.Start();
                Logger.LogInformation("TCP server started");
 
                while (true) {
                    TcpClient client = server.AcceptTcpClient();
                    Task.Run(() => ClientSession(new TCPProvider(client)));
                }
            }
            catch (Exception ex) {
                Logger.LogCritical(ex.Message);
                Logger.LogCritical(ex.InnerException?.Message);
            }
            finally {
                if (server != null)
                    server.Stop();
                Logger.LogInformation("Server stoped");
            }
        }
    }
}
