using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SimpleWallet
{
    class Program
    {
        private static string currentFileName = "";
        private static string currentPassword = "";
        private static WalletContainer currentWallet;

        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--help":
                    case "-h":
                        Console.WriteLine($"{Config.CurrencyName} - C# Wallet - v{ Config.Version }");
                        Console.WriteLine("=====   Commands   =====");
                        Console.WriteLine("--walletfile          : specifies the name of the wallet to open");
                        Console.WriteLine("--password            : the password to decrypt the wallet");
                        Console.WriteLine("--debug               : enables debugging output");
                        Console.WriteLine();
                        return;

                    case "--walletfile":
                        if (i + 1 < args.Length)
                        {
                            currentFileName = args[++i];
                        }
                        break;

                    case "--password":
                        if (i + 1 < args.Length)
                        {
                            currentPassword = args[++i];
                        }
                        break;
                }
            }

            Console.WriteLine("==============================");
            Console.WriteLine($"{Config.CurrencyName} - C# Wallet - v{ Config.Version }");
            Console.WriteLine("==============================");

            if (String.IsNullOrEmpty(currentPassword) || String.IsNullOrEmpty(currentFileName))
            {
                char responseChar = DisplayMenu(new Dictionary<char, string>
                {
                    {'O', "Open Existing Wallet"},
                    {'N', "Create new wallet"},
                    {'R', "Restore Wallet"},
                    {'X', "Exit"}
                });

                switch (Char.ToUpperInvariant(responseChar))
                {
                    case 'O':
                        GetWalletAndPassword(false);
                        currentWallet = WalletContainer.Read(currentFileName, currentPassword);
                        Console.WriteLine($"Opened wallet: {currentFileName}");
                        break;
                    case 'N':
                        GetWalletAndPassword(true);
                        currentWallet = new WalletContainer();
                        CreateNewWallet();
                        break;
                    case 'R':
                        GetWalletAndPassword(true);
                        currentWallet = new WalletContainer();
                        CreateNewWalletFromSeed();
                        break;
                    case 'X':
                        return;
                }
            }
            else
            {
                currentWallet = WalletContainer.Read(currentFileName, currentPassword);
            }

            new WalletSyncService(currentWallet, currentFileName, currentPassword).Start();

            while (true)
            {
                char mainResponseChar = DisplayMenu(new Dictionary<Char, String>
                {
                    { 'P', "Show pending transactions" },
                    { 'L', "List transfers" },
                    { 'B', "Balance" },
                    { 'T', "Transfer" },
                    { 'S', "Show seed" },
                    { 'R', "Rebuild wallet" },
                    { 'C', "Create new wallet" },
                    { 'A', "Add existing wallet" },
                    { 'D', "Delete wallet" },
                    { 'W', "List wallets" },
                    { 'N', "Name a wallet" },
                    { 'X', "Exit" }
                });

                switch (Char.ToUpperInvariant(mainResponseChar))
                {
                    case 'P':
                    case 'L':
                    case 'T':
                        Console.WriteLine("Feature not yet implemented");
                        break;
                    case 'B':
                        Console.WriteLine($"Current balance: { currentWallet.GetBalance() }");
                        break;
                    case 'S':
                        Console.WriteLine("Addresses and seeds - DO NOT SHARE:");
                        foreach (Wallet wallet in currentWallet.Wallets)
                        {
                            Console.WriteLine("-------------------------------------------------------------------------------------------------");
                            Console.WriteLine($"Address: {wallet.Address}");
                            Console.WriteLine($"Seed: {wallet.Seed}");
                        }
                        Console.WriteLine("-------------------------------------------------------------------------------------------------");
                        break;
                    case 'R':
                        foreach (Wallet wallet in currentWallet.Wallets)
                        {
                            wallet.Height = 0;
                            wallet.tokens.Clear();
                            wallet.Transactions.Clear();
                        }

                        currentWallet.Write(currentFileName, currentPassword);
                        break;

                    case 'C':
                        CreateNewWallet();
                        break;
                    case 'A':
                        CreateNewWalletFromSeed();
                        break;
                    case 'D':
                        for (var i = 0; i < currentWallet.Wallets.Count; i++)
                        {
                            Console.WriteLine($"{i}: {currentWallet.Wallets[i].Address}");
                        }

                        Console.Write("Please choose a wallet number to delete: ");
                        string readString = Console.ReadLine();
                        if (!String.IsNullOrWhiteSpace(readString) && readString.All(Char.IsDigit))
                        {
                            int readInt = Convert.ToInt32(readString);
                            if (readInt < currentWallet.Wallets.Count)
                            {
                                Console.WriteLine($"Wallet number {readInt} was removed!");
                                currentWallet.Wallets.RemoveAt(readInt);
                            }
                        }
                        else
                        {
                            Console.WriteLine("You did not enter a valid wallet number to delete!");
                        }

                        break;
                    case 'W':
                        for (var i = 0; i < currentWallet.Wallets.Count; i++)
                        {
                            Console.WriteLine($"{i}: {currentWallet.Wallets[i].Address}");
                        }
                        break;
                    case 'N':
                        for (var i = 0; i < currentWallet.Wallets.Count; i++)
                        {
                            Console.WriteLine($"{i}: {currentWallet.Wallets[i].Address}");
                        }

                        Console.Write("Please choose a wallet number to rename: ");
                        string readNameString = Console.ReadLine();
                        if (!String.IsNullOrWhiteSpace(readNameString) && readNameString.All(Char.IsDigit))
                        {
                            int readInt = Convert.ToInt32(readNameString);
                            if (readInt < currentWallet.Wallets.Count)
                            {
                                Console.Write("Please enter the new name for the wallet: ");
                                readNameString = Console.ReadLine();

                                if (!String.IsNullOrWhiteSpace(readNameString))
                                {
                                    currentWallet.Wallets[readInt].Name = readNameString;
                                    currentWallet.Write(currentFileName, currentPassword);
                                }
                                else
                                {
                                    Console.WriteLine("You did not enter a valid name!");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("You did not enter a valid wallet number to rename!");
                        }
                        break;
                    case 'X':
                        return;
                }
            }
        }

        private static void CreateNewWalletFromSeed()
        {
            string givenSeed;
            while (true)
            {
                Console.Write("Please enter your seed: ");
                givenSeed = Console.ReadLine();
                if (!String.IsNullOrWhiteSpace(givenSeed) && givenSeed.Length == 73)
                {
                    break;
                }
            }

            CreateNewWallet(givenSeed);
        }

        private static void CreateNewWallet(string guid = null)
        {
            using (SHA512 sha = SHA512.Create())
            {
                Wallet newWallet = new Wallet();
                if (guid == null)
                {
                    guid = Guid.NewGuid().ToString().ToLower() + '-' + Guid.NewGuid().ToString().ToLower();
                }
                byte[] guidBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(guid)).Take(32).ToArray();
                Keys keys = new Keys(guidBytes);
                newWallet.Address = keys.GetAddress();
                newWallet.Seed = guid;
                currentWallet.Wallets.Add(newWallet);
                currentWallet.Write(currentFileName, currentPassword);

                Console.WriteLine();
                Console.WriteLine("Wallet created - please record the following information somewhere secure. DO NOT SHARE THE SEED!");
                Console.WriteLine("-------------------------------------------------------------------------------------------------");
                Console.WriteLine($"Address: {newWallet.Address}");
                Console.WriteLine($"Seed: {newWallet.Seed}");
                Console.WriteLine("-------------------------------------------------------------------------------------------------");
            }
        }

        static void GetWalletAndPassword(bool repeatPassword)
        {
            while(String.IsNullOrWhiteSpace(currentFileName))
            {
                Console.Write("Please enter your wallet name: ");
                currentFileName = Console.ReadLine();
            }

            if (String.IsNullOrEmpty(currentPassword))
            {
                int i = 0;
                string enteredPassword = "";

                Console.Write("Please enter your password: ");

                do
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (!Char.IsControl(key.KeyChar))
                    {
                        enteredPassword += key.KeyChar;
                        Console.Write("*");
                    }
                    else if (key.Key == ConsoleKey.Backspace && enteredPassword.Length > 0)
                    {
                       enteredPassword = enteredPassword.Remove(enteredPassword.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (repeatPassword)
                        {
                            if (i == 0)
                            {
                                Console.WriteLine();
                                Console.Write("Enter your password again to confirm: ");
                                i++;
                                currentPassword = enteredPassword;
                                enteredPassword = "";
                            }
                            else if (i == 1)
                            {
                                if (String.Equals(currentPassword, enteredPassword))
                                {
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Password did not match. Try Again.");
                                    Console.Write("Please enter your password: ");
                                    i = 0;
                                    currentPassword = enteredPassword = "";
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                } while (true);

                currentPassword = enteredPassword;
                Console.WriteLine();
            }
        }

        static char DisplayMenu(Dictionary<Char, String> options)
        {
            while (true)
            {
                Console.WriteLine("----------------------------");
                Console.WriteLine();

                foreach (var option in options)
                {
                    Console.WriteLine($"[{option.Key}]: {option.Value}");
                }

                Console.WriteLine();
                Console.Write("Please choose one of the above options: ");
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (options.ContainsKey(Char.ToLowerInvariant(keyInfo.KeyChar)) || options.ContainsKey(Char.ToUpperInvariant(keyInfo.KeyChar)))
                {
                    Console.WriteLine();
                    return keyInfo.KeyChar;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("Please choose a valid option");
                    Console.WriteLine();
                }
            }
        }
    }
}
