using System.Security.Cryptography;
using System.Text;

namespace ZIKM.Server.Utils.StartupUtils {
    /// <summary>
    /// Decrypts configuration properties
    /// </summary>
    internal sealed class ConfigurationDecryptor {
        private readonly RSACryptoServiceProvider provider = new RSACryptoServiceProvider(2048);

        /// <summary>
        /// Create instance of <see cref="ConfigurationDecryptor"/>
        /// </summary>
        public ConfigurationDecryptor(string key) {
            provider.FromXmlString(key);
        }

        /// <summary>
        /// Decrypt property
        /// </summary>
        /// <param name="property">Encrypted property</param>
        /// <returns>Value of property</returns>
        public string Decrypt(string property) {
            var data = System.Array.ConvertAll(property.Split(';'), i => byte.Parse(i));

            return Encoding.UTF8.GetString(provider.Decrypt(data, false));
        }
    }
}
