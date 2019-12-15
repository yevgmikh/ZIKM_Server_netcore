using System;
using System.IO;
using ZIKM.Interfaces;

namespace ZIKM{
    /// <summary>
    /// Primitive captcha form perpeared .jpg files
    /// </summary>
    class PrimitiveCaptcha : ICaptcha{
        private static readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), "Captchas");
        private readonly TCPProvider provider;

        /// <summary>
        /// Create class for primitive captcha for TCP provider
        /// </summary>
        /// <param name="provider">Provider of client</param>
        public PrimitiveCaptcha(TCPProvider provider){
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Send captcha to client
        /// </summary>
        /// <param name="provider">Provider of client</param>
        /// <returns>Code of captcha</returns>
        public string SendCaptcha(){
            // Choose captcha
            string[] files = Directory.GetFiles(_path);
            var fileInfo = new FileInfo(files[new Random().Next(0, files.Length)]);
            string captcha = fileInfo.Name.Substring(0,5);

            provider.SendCaptcha(fileInfo.FullName);
            return captcha;
        }
    }
}