using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleWallet
{
    [Serializable]
    public class Wallet
    {
        public string Seed;
        public string Address;
        public uint Height;
        public Dictionary<String, WalletToken> tokens = new Dictionary<String, WalletToken>();
        public List<WalletTransaction> Transactions = new List<WalletTransaction>();
        public string Name;

        public float GetBalance()
        {
            return tokens.Values.Sum(tokenValue => (float)tokenValue.Value / Config.DenominationDivider);
        }
    }

    [Serializable]
    public class WalletToken
    {
        public string Token;
        public UInt32? Value;
    }

    [Serializable]
    public class WalletTransaction
    {
        public UInt32 Value = 0;
        public String Destination;
        public UInt64? Date;
        public String Ref;
    }
}
