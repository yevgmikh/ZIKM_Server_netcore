using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Services.Protectors {
    /// <summary>
    /// Encrypts the sending data of the <see cref="IProvider"/> by AES symmetric algorithm, 
    /// but sends encrypted by <see cref="RSA"/> algorithm key to client
    /// </summary>
    internal sealed class AesRsaProtector : IProtector {
        private readonly byte[] key = new byte[32];
        private readonly byte[] vector = new byte[16];

        /// <summary>
        /// Create <see cref="AesRsaProtector"/> class
        /// </summary>
        public AesRsaProtector() {
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            rng.GetBytes(vector);
        }

        public void ExchangeKey(ReadData read, SendData send) {
            using RSACryptoServiceProvider clientProvider = new RSACryptoServiceProvider(1024);
            clientProvider.FromXmlString(JsonSerializer.Deserialize<string>(read()));

            send(JsonSerializer.SerializeToUtf8Bytes(new Dictionary<string, byte[]>() {
                { "key", clientProvider.Encrypt(key, false) },
                { "vector", clientProvider.Encrypt(vector, false) }
            }));
            Logger.Log(LogMessages.KeyExchange);
        }

        public ReadOnlySpan<byte> Decrypt(byte[] data) {
            using AesManaged aesAlg = new AesManaged {
                Key = key,
                IV = vector
            };
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msDecrypt = new MemoryStream(data);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            byte[] buffer = new byte[1024];
            using MemoryStream ms = new MemoryStream();
            int numBytesRead;
            while ((numBytesRead = csDecrypt.Read(buffer, 0, buffer.Length)) > 0) {
                ms.Write(buffer, 0, numBytesRead);
            }
            return ms.ToArray();
        }

        public ReadOnlySpan<byte> Encrypt(byte[] data) {
            using AesManaged aesAlg = new AesManaged {
                Key = key,
                IV = vector
            };
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using MemoryStream msEncrypt = new MemoryStream();
            using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(data, 0, data.Length);
            csEncrypt.FlushFinalBlock();
            return msEncrypt.ToArray();
        }
    }
}
