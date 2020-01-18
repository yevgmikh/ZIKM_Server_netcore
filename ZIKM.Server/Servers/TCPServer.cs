using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZIKM.Servers.Providers;

namespace ZIKM.Servers {
    class TCPServer : ServerObject {
        public override void Start() {
            TcpListener server=null;
            try {
                server = new TcpListener(GetLocalIPAddress(), 8000);
 
                server.Start();
                Logger.ToLog("TCP server started");
 
                while (true) {
                    TcpClient client = server.AcceptTcpClient();
                    Task.Run(() => ClientSession(new TCPProvider(client)));
                }
            }
            catch (Exception ex) {
                Logger.ToLogAll(ex.Message);
                Logger.ToLogAll(ex.InnerException?.Message);
            }
            finally {
                if (server != null)
                    server.Stop();
                Logger.ToLog("Server stoped");
            }
        }
    }
}
