using System;
using System.Collections.Generic;

namespace ZIKM.Infrastructure.DataStructures {
    public struct RequestData {
        public RequestData(Guid sessionId, int operation, Dictionary<string, string> properties){
            SessionId = sessionId;
            Operation = operation;
            Properties = properties;
        }

        public Guid SessionId { get; set; }
        public int Operation { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
