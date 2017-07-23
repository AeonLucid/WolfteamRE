using System;
using System.Threading.Tasks;
using NLog;
using Wolfteam.Server.Config;

namespace Wolfteam.Server
{
    /// <summary>
    ///     Server to figure out how the client works.
    ///     TODO: Figure out how to connect the client to localhost.. (Editing the NyxLauncherEnc.xfs did not work..)
    /// </summary>
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main()
        {
            // Set NLog config.
            LogManager.Configuration = NLogConfig.Create();

            // Run program.
            Run().GetAwaiter().GetResult();

            // Catch exit.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static async Task Run()
        {
            // Set console title.
            Console.Title = "Wolfteam.Server";

            Logger.Info("Starting Wolfteam.Server.");
        }
    }
}
