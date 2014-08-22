using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Adaptive.ReactiveTrader.Client.Configuration;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Domain.Transport;
using Adaptive.ReactiveTrader.Shared;
using Adaptive.ReactiveTrader.Shared.DTO.Control;
using Autofac;
using log4net;
using ILoggerFactory = Adaptive.ReactiveTrader.Shared.Logging.ILoggerFactory;

namespace Adaptive.ReactiveTrader.ControlClient.CLI
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Program));

        static async void Main(string[] args)
        {
            InitializeLogging();

            var reactiveTraderApi = InitializeApi();

            await reactiveTraderApi.ConnectionStatusStream.Where(ci => ci.ConnectionStatus == ConnectionStatus.Connected);
            
            Log.Info("API Connected.");

            RunLoop(reactiveTraderApi);
        }

        private static IReactiveTrader InitializeApi()
        {
            string username;
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Username"]))
            {
                Console.WriteLine("Please enter your name (logging purposes only):");
                username = Console.ReadLine();
            }
            else
            {
                username = ConfigurationManager.AppSettings["Username"];
            }

            string authTokenKey;
            if (string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[AuthTokenProvider.AuthTokenKey]))
            {
                Console.WriteLine("Please enter your authorization token:");
                authTokenKey = Console.ReadLine();
            }
            else
            {
                authTokenKey = ConfigurationManager.AppSettings[AuthTokenProvider.AuthTokenKey];
            }

            var bootstrapper = new Bootstrapper(username);
            var container = bootstrapper.Build();

            Log.Info("Initializing reactive trader API...");
            var sw = Stopwatch.StartNew();
            var reactiveTraderApi = container.Resolve<IReactiveTrader>();

            reactiveTraderApi.Initialize(username, container.Resolve<IConfigurationProvider>().Servers, container.Resolve<ILoggerFactory>(), authTokenKey);
            Log.InfoFormat("Reactive trader API initialized in {0}ms", sw.ElapsedMilliseconds);
            
            return reactiveTraderApi;
        }

        private static async void RunLoop(IReactiveTrader reactiveTraderApi)
        {
            while (true)
            {
                var states = await reactiveTraderApi.Control.GetCurrencyPairStates();
                Print(states);
                await SendCommand(states, reactiveTraderApi);
            }
        }

        private static void Print(IEnumerable<CurrencyPairStateDto> states)
        {
            Console.WriteLine("Symbol   | (E)nabled/(D)isable | (A)ctive/(S)tale   |");
            foreach (var state in states.OrderBy(st => st.Symbol))
            {
                Console.WriteLine("{0, -9} | {1, 20} | {2, 20} |", state.Symbol, 
                    state.Enabled
                    ? "E"
                    : "D",
                    state.Stale
                    ? "S"
                    : "A");
            }
        }

        private async static Task SendCommand(IEnumerable<CurrencyPairStateDto> states, IReactiveTrader reactiveTrader)
        {
            var commandLine = Console.ReadLine();
            var args = commandLine.Split(new [] { ' '}, StringSplitOptions.RemoveEmptyEntries);
            
            var ccyPair = states.FirstOrDefault(state => string.Equals(args[0], state.Symbol, StringComparison.InvariantCultureIgnoreCase));

            if (ccyPair == null)
            {
                Console.WriteLine("Could not find symbol {0}", args[0]);
                return;
            }

            bool? setEnabled = null, setStale = null;

            foreach (var arg in args.Skip(1))
            {
                switch (arg.ToUpperInvariant())
                {
                    case "S":
                        setStale = true;
                        break;
                    case "A":
                        setStale = false;
                        break;
                    case "E":
                        setEnabled = true;
                        break;
                    case "D":
                        setEnabled = false;
                        break;
                }
            }

            Console.WriteLine("From: {0}", ccyPair);
            if (setEnabled.HasValue)
            {
                ccyPair.Enabled = setEnabled.Value;
            }

            if (setStale.HasValue)
            {
                ccyPair.Stale = setStale.Value;
            }
            Console.WriteLine("To:   {0}", ccyPair);
            Console.WriteLine("Setting..");
            try
            {
                var result =
                    await reactiveTrader.Control.SetCurrencyPairState(ccyPair.Symbol, ccyPair.Enabled, ccyPair.Stale);
                Console.WriteLine("Set!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to set.");
                Console.WriteLine(ex);
            }
        }


        private static void InitializeLogging()
        {
            log4net.Config.XmlConfigurator.Configure();

            Log.Info(@"  ____  ____    __    ___  ____  ____  _  _  ____  ");
            Log.Info(@" (  _ \( ___)  /__\  / __)(_  _)(_  _)( \/ )( ___) ");
            Log.Info(@"  )   / )__)  /(__)\( (__   )(   _)(_  \  /  )__)  ");
            Log.Info(@" (_)\_)(____)(__)(__)\___) (__) (____)  \/  (____)");
            Log.Info(@"   ___  _____  _  _  ____  ____  _____  __  ");
            Log.Info(@"  / __)(  _  )( \( )(_  _)(  _ \(  _  )(  ) ");
            Log.Info(@" ( (__  )(_)(  )  (   )(   )   / )(_)(  )(__");
            Log.Info(@"  \___)(_____)(_)\_) (__) (_)\_)(_____)(____)"); 
        }
    }
}
