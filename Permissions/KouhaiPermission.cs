using System;
using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    class KouhaiPermission : Client{
        protected override string EndMessage { get; set; } = "Be carefull, my kouhai.";
        protected override string EndLog { get; set; } = "Pervered kouhai gone";

        public KouhaiPermission(IProvider provider, Guid guid): base(provider, guid) { 
            code = 0; 
        }

        /// <summary>
        /// Start session for kouhai
        /// </summary>
        public override void StartSession(){
            _provider.SendResponse(new ResponseData(sessionid, 0, "Sempai is waitting you)"));
            Logger.ToLog("Pervered kouhai here");
            Session(2);
        }
    }
}