using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using ZIKM.Server.Infrastructure.Interfaces;

namespace ZIKM.Server.Utils.StartupUtils {
    /// <summary>
    /// Retrieves the keys to decrypt the configuration parameters
    /// </summary>
    internal sealed class KeyLoader {
        private readonly string ipAddress;
        private readonly IProtector protector;

        /// <summary>
        /// Create <see cref="KeyLoader"/> instance
        /// </summary>
        /// <param name="ipAddress">IP address or hostname of sourse</param>
        /// <param name="protector">Data protector</param>
        public KeyLoader(string ipAddress, IProtector protector) {
            this.ipAddress = ipAddress ?? throw new ArgumentNullException(nameof(ipAddress));
            this.protector = protector ?? throw new ArgumentNullException(nameof(protector));
        }

        public Dictionary<string, string> GetKeys() {
            var client = new TcpClient(ipAddress, 9000);
            var stream = client.GetStream();

            byte[] read() {
                using MemoryStream ms = new MemoryStream();
                byte[] data = new byte[1024];
                do {
                    int numBytesRead = stream.Read(data, 0, data.Length);
                    ms.Write(data, 0, numBytesRead);
                } while (stream.DataAvailable);
                return ms.ToArray();
            };

            protector.ExchangeKey(read, (ReadOnlySpan<byte> data) => {
                stream.Write(data);
            });

            return JsonSerializer.Deserialize<Dictionary<string, string>>(protector.Decrypt(read()));
        }
    }
}
