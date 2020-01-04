using System;
using ZIKM.Infrastructure.Enums;

namespace ZIKM.Infrastructure.DataStructures{
    public struct ResponseData{
        public ResponseData(StatusCode code, string message) : this(){
            Code = code;
            Message = message;
        }

        public ResponseData(Guid sessionId, StatusCode code, string message){
            SessionId = sessionId;
            Code = code;
            Message = message;
        }

        public Guid SessionId { get; set; }
        public StatusCode Code { get; set; }
        public string Message { get; set; }
    }
}
