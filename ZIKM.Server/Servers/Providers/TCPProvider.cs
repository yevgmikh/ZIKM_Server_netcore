using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Servers.Providers {
    public class TCPProvider : IProvider {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly IProtector _protector;
        private readonly ProviderLogger _logger = ProviderLogger.Instance;

        public TCPProvider(TcpClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = _client.GetStream();
            _protector = IoC.GetService<IProtector>();
        }

        /// <summary>
        /// Read client request
        /// </summary>
        /// <returns>Data of client request</returns>
        private byte[] ReadRequest() {
            using MemoryStream ms = new MemoryStream();
            byte[] data = new byte[1024];
            do {
                int numBytesRead = _stream.Read(data, 0, data.Length);
                ms.Write(data, 0, numBytesRead);
            } while (_stream.DataAvailable);
            return ms.ToArray();
        }

        /// <summary>
        /// Send data to client
        /// </summary>
        /// <param name="data">Sent data</param>
        private void SendData(ReadOnlySpan<byte> data) {
            _stream.Write(data);
        }

        public void PrepareProtecting() {
            _protector.ExchangeKey(ReadRequest, SendData);
        }

        public RequestData GetRequest()
        {
            var data = _protector.Decrypt(ReadRequest());
            _logger.ToLogProvider(data);
            return JsonSerializer.Deserialize<RequestData>(data);
        }

        public LoginData GetLoginRequest() {
            var data = _protector.Decrypt(ReadRequest());
            _logger.ToLogProvider(data);
            return JsonSerializer.Deserialize<LoginData>(data);
        }

        public void SendResponse(ResponseData response) {
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(response);
            SendData(_protector.Encrypt(data));
            _logger.ToLogProvider(data);
        }

        public void SendCaptcha(byte[] fileData) {
            SendData(_protector.Encrypt(fileData));
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) { }

                _stream.Close();
                _client.Close();

                disposedValue = true;
            }
        }

        ~TCPProvider() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}