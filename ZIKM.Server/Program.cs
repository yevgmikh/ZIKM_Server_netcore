using ZIKM.Server.Servers;

namespace ZIKM.Server {
    class Program {
        static void Main(string[] args) {
            ServerObject server = new TCPServer();
            server.Start();
        }
    }
}
