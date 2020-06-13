using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Servers.Tcp {
    class TCPServer : ServerObject {
        protected override void HandleErrors(Action operation) {
            try {
                operation();
            }
            catch (IOException ex) when (ex.InnerException is SocketException inner && inner.ErrorCode == 10054) {
                Logger.LogAll(LogMessages.LostConnection);
            }
            catch (Exception ex) {
                Logger.LogError(ex.Message);
                Logger.LogError(ex.InnerException?.Message);
            }
        }

        protected override void HandleDisconnect(Action operation, Action<Exception> handler) {
            try {
                operation();
            }
            catch (IOException ex) when (ex.InnerException is SocketException inner && inner.ErrorCode == 10054) {
                throw;
            }
            catch (Exception ex) {
                handler(ex);
            }
        }

        protected override bool HandleDisconnect(Func<bool> operation, Func<Exception, bool> handler) {
            try {
                return operation();
            }
            catch (IOException ex) when (ex.InnerException is SocketException inner && inner.ErrorCode == 10054) {
                throw;
            }
            catch (Exception ex) {
                return handler(ex);
            }
        }

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
