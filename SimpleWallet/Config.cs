using System;

namespace SimpleWallet
{
    public static class Config
    {
        public const String Version = "0.0.5.1";
        public const String CurrencyName = "Veldspar";
        public const String CurrencyNetworkAddress = "VE";
        //public const String GenesisID = "0fcbb8951fd052764f71a634b02361448386c5b0f70eadb716cc0f3f";
        //public const Int64 BlockchainStartDate = 1533513599000;

        //public const Byte MagicByte = 255;

        //public const Int32 TransactionMaturityLevel = 5;

        public const Int32 DenominationDivider = 100;

        //public const Int32 BlockTime = 60 * 2;

        //public const Int32 OreSize = 1;

        //public const Int32 OreReleasePoint = 250000;

        //public const Int32 TokenSegmentSize = 64;

        //public const Int32 TokenAddressSize = 8;

        public static readonly string[] SeedNodes = { "138.68.116.96" }; 
    }
}
