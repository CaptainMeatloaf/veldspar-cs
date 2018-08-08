using System;
using System.Linq;
using SimpleBase;

namespace SimpleWallet
{
    public class Keys
    {
        public byte[] PrivateKey;
        public byte[] PublicKey;

        public Keys(byte[] seed)
        {
            PublicKey = Ed25519.PublicKey(seed);
            PrivateKey = seed.Concat(PublicKey).ToArray();
        }

        public String GetAddress()
        {
            return $"{Config.CurrencyNetworkAddress}{Base58.Bitcoin.Encode(PublicKey)}";
        }
    }
}
