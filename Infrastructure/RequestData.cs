using System;

namespace ZIKM.Infrastructure{
    public struct RequestData{
        public RequestData(Guid sessionId, int operation, string property){
            SessionId = sessionId;
            Operation = operation;
            Property = property;
        }

        public Guid SessionId { get; set; }
        public int Operation { get; set; }
        public string Property { get; set; }

        public override string ToString() => $"SessionId: {SessionId}, Operation: {Operation}, Property: {Property}";
    }
}
