using System;

namespace ZIKM.Infrastructure.DataStructures{
    public struct ResponseData{
        public ResponseData(short code, string message) : this(){
            Code = code;
            Message = message;
        }

        public ResponseData(Guid sessionId, short code, string message){
            SessionId = sessionId;
            Code = code;
            Message = message;
        }

        public Guid SessionId { get; set; }
        public short Code { get; set; }
        public string Message { get; set; }
    }
}
