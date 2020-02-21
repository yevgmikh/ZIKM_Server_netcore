using System;
using System.IO;
using System.Security.Cryptography;

namespace ZIKM.Server.Utils {
    class ProviderLogger {
        private static readonly string logProviderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogProvider");
        /// <summary>
        /// Get instance of <see cref="ProviderLogger"/>
        /// </summary>
        public static ProviderLogger Instance { get; protected set; }

        /// <summary>
        /// Create instance of <see cref="ProviderLogger"/>
        /// </summary>
        /// <param name="key"></param>
        public static void CreateInstance(string key, string vector) {
            if (Instance == null)
                Instance = new ProviderLogger(key, vector);
            Directory.CreateDirectory(logProviderPath);
        }

        private readonly byte[] key = new byte[32];
        private readonly byte[] vector = new byte[16];

        private ProviderLogger(string key, string vector) {
            this.key = Array.ConvertAll(key.Split(';'), i => byte.Parse(i));
            this.vector = Array.ConvertAll(vector.Split(';'), i => byte.Parse(i));
        }

        /// <summary>
        /// Write provider comunication operation
        /// </summary>
        /// <param name="data">Log entry</param>
        public void ToLogProvider(ReadOnlySpan<byte> data) {
            using FileStream stream = new FileStream(Path.Combine(logProviderPath, $"{DateTime.Today.ToString("dd-MM-yyyy")}.log"), FileMode.OpenOrCreate);
            using AesManaged aesAlg = new AesManaged {
                Key = key,
                IV = vector
            };
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using MemoryStream msEncrypt = new MemoryStream();
            using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(data);
            csEncrypt.FlushFinalBlock();

            stream.Write(msEncrypt.ToArray());
        }
    }
}
