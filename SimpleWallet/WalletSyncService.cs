using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleWallet
{
    public class WalletSyncService
    {
        private WalletContainer walletContainer;
        private String filePath;
        private string password;
        private Task backgroundTask;

        public WalletSyncService(WalletContainer wallet, String filePath, String password)
        {
            this.walletContainer = wallet;
            this.filePath = filePath;
            this.password = password;
        }

        public void Start()
        {
            backgroundTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    foreach (Wallet wallet in walletContainer.Wallets)
                    {
                        uint walletHeight = 0;
                        String address;

                        lock (walletContainer)
                        {
                            walletHeight = wallet.Height;
                            address = wallet.Address;
                        }

                        try
                        {
                            string outputJson = await Comms.RequestJson("wallet/sync",
                                new Dictionary<String, String>
                                {
                                    {"height", walletHeight.ToString()},
                                    {"address", address}
                                });

                            if (!String.IsNullOrWhiteSpace(outputJson))
                            {
                                Dictionary<String, List<RPCLedger>> grouped = new Dictionary<String, List<RPCLedger>>();

                                RPCWalletSyncObject syncObject = JsonConvert.DeserializeObject<RPCWalletSyncObject>(outputJson);
                                if (syncObject.transactions.Any())
                                {
                                    uint totalAdded = 0;
                                    uint totalSpent = 0;

                                    lock (walletContainer)
                                    {
                                        foreach (RPCLedger ledger in syncObject.transactions)
                                        {
                                            if (ledger.destination == address)
                                            {
                                                WalletToken token = new WalletToken();
                                                token.Token = ledger.token;
                                                token.Value = Convert.ToUInt32(ledger.token.Split('-')[2], 16);

                                                wallet.tokens[ledger.token] = token;

                                                totalAdded += token.Value.Value;
                                            }
                                            else
                                            {
                                                // is this a transfer out of a token which was owned by this wallet?
                                                if (wallet.tokens.ContainsKey(ledger.token))
                                                {
                                                    WalletToken token = wallet.tokens[ledger.token];
                                                    wallet.tokens.Remove(ledger.token);

                                                    if (!grouped.ContainsKey(ledger.transaction_group))
                                                    {
                                                        grouped[ledger.transaction_group] = new List<RPCLedger>();
                                                    }

                                                    grouped[ledger.transaction_group].Add(ledger);

                                                    totalSpent += token.Value.Value;
                                                }
                                            }
                                        }

                                        foreach (var transaction in grouped)
                                        {
                                            WalletTransaction newTransaction = new WalletTransaction();
                                            newTransaction.Ref = transaction.Key;
                                            newTransaction.Date = transaction.Value[0].date;
                                            newTransaction.Destination = transaction.Value[0].destination;

                                            foreach (RPCLedger ledger in transaction.Value)
                                            {
                                                newTransaction.Value += Convert.ToUInt32(ledger.token.Split('-')[2], 16);
                                            }

                                            wallet.Transactions.Add(newTransaction);
                                        }

                                        wallet.Height = (uint)syncObject.rowid;
                                    }

                                    walletContainer.Write(filePath, password);

                                    Console.WriteLine($"{(float)totalAdded / Config.DenominationDivider} {Config.CurrencyName} added to wallet");
                                    Console.WriteLine($"Value of spent tokens: {(float)totalSpent / Config.DenominationDivider}");
                                    Console.WriteLine("====================");
                                    Console.WriteLine($"Current balance: {wallet.GetBalance()}");
                                }
                                else
                                {
                                    Debug.WriteLine("Height: " + walletHeight);
                                    await Task.Delay(10000);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("An error occurred");
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
