using ZIKM.Servers;

namespace ZIKM {
    class Program {
        static void Main(string[] args) {
            ServerObject server = new TCPServer();
            server.Start();
        }
    }
}
