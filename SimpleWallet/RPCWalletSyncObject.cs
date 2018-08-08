using System;
using System.Collections.Generic;

namespace SimpleWallet
{
    [Serializable]
    public class RPCWalletSyncObject
    {
        public Int32 rowid = 0;
        public List<RPCLedger> transactions;
    }
}
