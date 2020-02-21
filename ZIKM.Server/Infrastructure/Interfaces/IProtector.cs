using System;

namespace ZIKM.Server.Infrastructure.Interfaces {
    /// <summary>
    /// Encrypts the sending data of the <see cref="IProvider"/>
    /// </summary>
    internal interface IProtector {
        /// <summary>
        /// Exchange cryptography keys with the client
        /// </summary>
        /// <param name="read">Reader client data</param>
        /// <param name="send">Sender data to client</param>
        void ExchangeKey(ReadData read, SendData send);
        /// <summary>
        /// Encrypt data
        /// </summary>
        /// <param name="data">Data to encrypt</param>
        /// <returns>Encrypt data</returns>
        ReadOnlySpan<byte> Encrypt(byte[] data);
        /// <summary>
        /// Decrypt data
        /// </summary>
        /// <param name="data">Data to decrypt</param>
        /// <returns>Decrypt data</returns>
        ReadOnlySpan<byte> Decrypt(byte[] data);
    }

    delegate byte[] ReadData();
    delegate void SendData(ReadOnlySpan<byte> data);
}
