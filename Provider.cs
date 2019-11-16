using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM{
    class TCPProvider : IProvider{
        private byte[] _data;
        private StringBuilder _response;
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public TCPProvider(TcpClient client){
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = _client.GetStream();
        }

        /// <summary>
        /// Send response to client
        /// </summary>
        /// <param name="response">Response data</param>
        public void SendResponse(ResponseData response){
            string JsonString = JsonConvert.SerializeObject(response);
            _data = Encoding.UTF8.GetBytes(JsonString);
            _stream.Write(_data, 0, _data.Length);
            Logger.ToLogProvider(JsonString);
        }

        /// <summary>
        /// Send captcha to client
        /// </summary>
        /// <param name="filePath">Path to captcha-file</param>
        public void SendCaptcha(string filePath){
            _data = File.ReadAllBytes(filePath);
            _stream.Write(_data, 0, _data.Length);
        }

        /// <summary>
        /// Read client request
        /// </summary>
        /// <returns>JSON-string with data of clint request</returns>
        private string ReadRequest(){
            _data = new byte[100000];
            _response = new StringBuilder();
            int bytes = _stream.Read(_data, 0, _data.Length);
            _response.Append(Encoding.UTF8.GetString(_data, 0, bytes));
            Logger.ToLogProvider(_response.ToString());
            return _response.ToString();
        }

        /// <summary>
        /// Get data from client request
        /// </summary>
        /// <returns>Data of client's request</returns>
        public RequestData GetRequest(){
            return JsonConvert.DeserializeObject<RequestData>(ReadRequest());
        }

        /// <summary>
        /// Get data from client login-request
        /// </summary>
        /// <returns>Data of login request</returns>
        public LoginData GetLoginRequest(){
            return JsonConvert.DeserializeObject<LoginData>(ReadRequest());
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing){
            if (!disposedValue){
                if (disposing){
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
        ~TCPProvider(){
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose(){
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}