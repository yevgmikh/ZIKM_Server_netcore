namespace ZIKM.Infrastructure.Interfaces {
    internal interface ICaptcha {
        /// <summary>
        /// Get captcha to send to the client
        /// </summary>
        /// <param name="code">Captcha code</param>
        /// <returns>Image to send</returns>
        byte[] GetCaptcha(out string code);
    }
}
