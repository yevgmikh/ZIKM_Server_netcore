using System;
using System.IO;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Services.Captcha {
    /// <summary>
    /// Simple captcha form perpeared .jpg files
    /// </summary>
    internal class SimpleCaptcha : ICaptcha {
        /// <summary>
        /// Get instance of simple captcha class
        /// </summary>
        public static SimpleCaptcha Instance { get; } = new SimpleCaptcha();

        private static readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Captchas");

        /// <summary>
        /// Create class for simple captcha
        /// </summary>
        private SimpleCaptcha() { }

        public byte[] GetCaptcha(out string code) {
            // Choose captcha
            string[] files = Directory.GetFiles(_path);
            var fileInfo = new FileInfo(files[new Random().Next(0, files.Length)]);
            code = fileInfo.Name.Split('.')[0];

            return File.ReadAllBytes(fileInfo.FullName);
        }
    }
}