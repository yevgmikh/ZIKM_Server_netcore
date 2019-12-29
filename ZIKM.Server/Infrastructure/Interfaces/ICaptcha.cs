namespace ZIKM.Infrastructure.Interfaces{
    public interface ICaptcha{
        /// <summary>
        /// Send captcha to client
        /// </summary>
        /// <returns>Code of captcha</returns>
        string SendCaptcha();
    }
}
