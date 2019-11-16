using System;
using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    class SenpaiPermission : Client{
        protected override string EndMessage { get; set; } = "Senpai! I will wait!!!";
        protected override string EndLog { get; set; } = "Sempai gone";

        public SenpaiPermission(IProvider provider, Guid guid): base(provider,guid) { 
            code = 0; 
        }

        /// <summary>
        /// Start session for senpai
        /// </summary>
        public override void StartSession(){
            _provider.SendResponse(new ResponseData(sessionid, 0, "Senpai!!!XD"));
            Logger.ToLog("Sempai back");
            Session(3);
        }
    }
}