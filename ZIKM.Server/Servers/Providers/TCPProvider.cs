using System;
using System.Net.Sockets;
using System.Text.Json;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Servers.Providers {
    public class TCPProvider : IProvider {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public TCPProvider(TcpClient client) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = _client.GetStream();
        }

        /// <summary>
        /// Send response to client
        /// </summary>
        /// <param name="response">Response data</param>
        public void SendResponse(ResponseData response) {
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(response);
            _stream.Write(data, 0, data.Length);
            Logger.ToLogProvider(data, data.Length);
        }

        /// <summary>
        /// Send captcha to client
        /// </summary>
        /// <param name="fileData">Image to send</param>
        public void SendCaptcha(byte[] fileData) {
            _stream.Write(fileData, 0, fileData.Length);
        }

        /// <summary>
        /// Read client request
        /// </summary>
        /// <returns>Data of clint request</returns>
        private ReadOnlySpan<byte> ReadRequest() {
            byte[] data = new byte[_client.SendBufferSize];
            int bytes = _stream.Read(data);
            Logger.ToLogProvider(data, bytes);
            return new ReadOnlySpan<byte>(data, 0, bytes);
        }

        /// <summary>
        /// Get data from client request
        /// </summary>
        /// <returns>Data of client's request</returns>
        public RequestData GetRequest() {
            return JsonSerializer.Deserialize<RequestData>(ReadRequest());
        }

        /// <summary>
        /// Get data from client login-request
        /// </summary>
        /// <returns>Data of login request</returns>
        public LoginData GetLoginRequest() {
            return JsonSerializer.Deserialize<LoginData>(ReadRequest());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _stream.Close();
                _client.Close();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~TCPProvider() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}