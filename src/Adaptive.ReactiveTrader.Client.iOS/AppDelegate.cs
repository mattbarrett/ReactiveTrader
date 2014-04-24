using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Xsl;
using Adaptive.ReactiveTrader.Client.Domain.Models;
using Adaptive.ReactiveTrader.Client.Domain.Models.Pricing;
using Adaptive.ReactiveTrader.Client.Domain.Models.ReferenceData;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Adaptive.ReactiveTrader.Client.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        UIWindow window;
        private Domain.ReactiveTrader _reactiveTrader;
        private RootElement _rootElement;
        private Section _trades;
        private Section _prices;
        private Section _section;
        private ConcurrencyService.ConcurrencyService _concurrencyService;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            _reactiveTrader = new Domain.ReactiveTrader();
            _reactiveTrader.Initialize("matt", new[] { "http://reactivetrader.azurewebsites.net/signalr" });

            _concurrencyService = new ConcurrencyService.ConcurrencyService();

            _prices = new Section("Prices");
            _trades = new Section("Trades");

            _section = new Section("Connecting..")
            {
                new RootElement("Prices") {_prices},
                new RootElement("Trades") {_trades}
            };

            _rootElement = new RootElement("Reactive Trader")
            {
                _section
            };

            _reactiveTrader.ConnectionStatusStream
                .ObserveOn(_concurrencyService.Dispatcher)
                .SubscribeOn(_concurrencyService.ThreadPool)
                .Subscribe(ci =>
                {
                    _section.Caption = ci.ConnectionStatus.ToString();
                });
                
            _reactiveTrader.TradeRepository.GetTradesStream()
                .ObserveOn(_concurrencyService.Dispatcher)
                .SubscribeOn(_concurrencyService.ThreadPool)
                .Subscribe(trades =>
                {
                    foreach (var trade in trades)
                    {
                        _trades.Add(new MultilineElement(trade.TradeStatus.ToString(), trade.ToString()));
                    }
                });

            
            _reactiveTrader.ReferenceData.GetCurrencyPairsStream()
                .SelectMany(_ => _)
                .SubscribeOn(_concurrencyService.ThreadPool)
                .ObserveOn(_concurrencyService.Dispatcher)
                .Subscribe(cpu =>
                {
                    switch (cpu.UpdateType)
                    {
                        case UpdateType.Add:
                            AddCurrencyPair(cpu);
                            break;
                        case UpdateType.Remove:
                            RemoveCurrencyPair(cpu);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });


            var dvc = new DialogViewController(_rootElement);

            window.RootViewController = dvc;

            window.MakeKeyAndVisible();

            return true;
        }

        private void AddCurrencyPair(ICurrencyPairUpdate cpu)
        {
            var rootElement = new RootElement(cpu.CurrencyPair.Symbol);
            _prices.Add(rootElement);

            IPrice lastPrice = null;
            var sell = new StringElement("SELL", delegate 
            {
                if (lastPrice != null && !lastPrice.IsStale)
                {
                    lastPrice.Bid.ExecuteRequest(1000, cpu.CurrencyPair.BaseCurrency)
                        .ObserveOn(_concurrencyService.Dispatcher)
                        .SubscribeOn(_concurrencyService.ThreadPool)
                        .Subscribe(_ =>
                        {

                        });
                }
            });
            
            var buy = new StringElement("BUY", delegate
            {
                if (lastPrice != null && !lastPrice.IsStale)
                {
                    lastPrice.Ask.ExecuteRequest(1000, cpu.CurrencyPair.BaseCurrency)
                        .SubscribeOn(_concurrencyService.ThreadPool)
                        .ObserveOn(_concurrencyService.Dispatcher)
                        .Subscribe(_ => { });
                }
            });

            var section = new Section() {sell, buy};

            rootElement.Add(section);
            cpu.CurrencyPair.PriceStream
                .SubscribeOn(_concurrencyService.ThreadPool)
                .ObserveOn(_concurrencyService.Dispatcher)
                .Subscribe(price =>
            {
                lastPrice = price;
                if (price.IsStale)
                {
                    sell.Value = buy.Value = "STALE";
                }
                else
                {
                    sell.Value = price.Bid.Rate.ToString();
                    buy.Value = price.Ask.Rate.ToString();
                }
            });
        }

        private void RemoveCurrencyPair(ICurrencyPairUpdate cpu)
        {
            var element = _prices.Elements.Cast<RootElement>().FirstOrDefault(re => re.Caption == cpu.CurrencyPair.Symbol);
            if (element != null)
                _prices.Elements.Remove(element);
        }
    }
}

