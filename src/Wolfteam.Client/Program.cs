using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Wolfteam.Client.Config;
using Wolfteam.Client.Net.Wolfteam;

namespace Wolfteam.Client
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        private static void Main()
        {
            // Set console title.
            Console.Title = "Wolfteam.Client";
            Console.CancelKeyPress += (sender, args) =>
            {
                args.Cancel = true;
                QuitEvent.Set();
            };

            Console.WriteLine("Press CTRL+C to exit.");

            // Set NLog config.
            LogManager.Configuration = NLogConfig.Create();

            // Run program.
            Run().GetAwaiter().GetResult();

            // Wait until cancel.
            QuitEvent.WaitOne();
        }

        private static async Task Run()
        {
            // Announce startup.
            Logger.Info("Starting Wolfteam.Client.");

            // Load accounts.
            var accountsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Accounts.json");
            if (!File.Exists(accountsPath))
            {
                Logger.Error("No accounts.json file found in the same directory as the assembly.");
                return;
            }

            // Account indexes (My accounts file; 0 = Unknown account, 1 = Real account, 2 = Real empty account)
            var accounts = JsonConvert.DeserializeObject<AccountConfig[]>(File.ReadAllText(accountsPath));

            // Initiate session. 
            var session = new Session(accounts[2]);

            Logger.Info("Connecting to Wolfteam.");

            if (!await session.ConnectAsync())
            {
                Logger.Error("Could not connect to Wolfteam.");
            }

            Logger.Info("Connected to Wolfteam.");

            // Start listening for a response.
            Logger.Info("Starting listening thread.");

            session.StartListening();

            // Send login packet.
            Logger.Info("Sending login packet.");

            await session.LoginAsync();
        }
    }
}
