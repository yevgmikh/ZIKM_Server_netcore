using System;
using System.Collections.Generic;
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
                byte[] bufferSize = new byte[4];
                stream.Read(bufferSize, 0, 4);

                byte[] data = new byte[BitConverter.ToInt32(bufferSize, 0)];
                stream.Read(data, 0, data.Length);
                return data;
            };

            protector.ExchangeKey(read, (ReadOnlySpan<byte> data) => {
                stream.Write(BitConverter.GetBytes(data.Length));
                stream.Write(data);
            });

            return JsonSerializer.Deserialize<Dictionary<string, string>>(protector.Decrypt(read()));
        }
    }
}
