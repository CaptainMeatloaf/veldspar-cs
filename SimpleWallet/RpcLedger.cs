using System;

namespace SimpleWallet
{
    [Serializable]
    public class RPCLedger
    {
        public String transaction_id;
        public Int32? op;
        public UInt64? date;
        public String transaction_group;
        public String destination;
        public string token;
        public string spend_auth;
        public UInt32? block;
    }
}
