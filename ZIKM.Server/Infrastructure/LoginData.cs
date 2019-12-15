namespace ZIKM.Infrastructure{
    public struct LoginData{
        public LoginData(string user, string password, string captcha){
            User = user;
            Password = password;
            Captcha = captcha;
        }

        public string User { get; set; }
        public string Password { get; set; }
        public string Captcha { get; set; }

        public override string ToString() => $"User: {User}, Password: {Password}, Captcha: {Captcha}";
    }
}
