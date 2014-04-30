using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Domain.Models.ReferenceData;
using Adaptive.ReactiveTrader.Client.Domain.Models.Pricing;
using System.Reactive.Linq;
using System.Linq;
using Adaptive.ReactiveTrader.Client.Concurrency;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public partial class PricesViewController : UIViewController
	{
		readonly IReactiveTrader _reactiveTrader;
		readonly IConcurrencyService _concurrencyService;

		Section _ccyPairs;

		public PricesViewController (IReactiveTrader reactiveTrader, IConcurrencyService concurrencyService) : base ("SecondViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Prices", "Prices");
			TabBarItem.Image = UIImage.FromBundle ("second");

			_reactiveTrader = reactiveTrader;
			_concurrencyService = concurrencyService;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var root = new RootElement ("Prices");
			_ccyPairs = new Section ("Currency Pairs");

			_reactiveTrader.ReferenceData
				.GetCurrencyPairsStream ()
				.Subscribe (updates => BeginInvokeOnMainThread (() => {
				foreach (var update in updates) {
					if (update.UpdateType == Adaptive.ReactiveTrader.Client.Domain.Models.UpdateType.Add) {
						AddCurrencyPair (update);
					} else {
						RemoveCurrencyPair (update);
					}
				}
			}));

			root.Add (_ccyPairs);

			var dvc = new DialogViewController (root, true);
			var nav = new UINavigationController (dvc);
			this.AddChildViewController (nav);
			this.Add (nav.View);
		}


		private void AddCurrencyPair(ICurrencyPairUpdate cpu)
		{
			var rootElement = new RootElement(cpu.CurrencyPair.Symbol);
			_ccyPairs.Add(rootElement);

			IPrice lastPrice = null;
			var sell = new StringElement ("SELL", delegate
				{
					if (lastPrice != null && !lastPrice.IsStale)
					{
						lastPrice.Bid.ExecuteRequest(1000, cpu.CurrencyPair.BaseCurrency)
							.ObserveOn(_concurrencyService.Dispatcher)
							.SubscribeOn(_concurrencyService.ThreadPool)
							.Where(t => !t.IsStale)
							.Subscribe(trade =>
								{
									var view = new UIAlertView () {
										Title = "Trade Request",
										Message = trade.Update.ToString ()
									};
									view.AddButton ("OK");
									view.Show ();

								});
					}
				});

			var buy = new StringElement ("BUY", delegate
				{
					if (lastPrice != null && !lastPrice.IsStale)
					{
						lastPrice.Ask.ExecuteRequest(1000, cpu.CurrencyPair.BaseCurrency)
							.ObserveOn(_concurrencyService.Dispatcher)
							.SubscribeOn(_concurrencyService.ThreadPool)
							.Where(t => !t.IsStale)
							.Subscribe(trade =>
								{
									var view = new UIAlertView () {
										Title = "Trade Request",
										Message = trade.Update.ToString ()
									};
									view.AddButton ("OK");
									view.Show ();

								});
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
						RefreshElement (buy);
						RefreshElement (sell);
					});
		}

		private void RemoveCurrencyPair(ICurrencyPairUpdate cpu)
		{
			var element = _ccyPairs.Elements.OfType<RootElement> ()
				.FirstOrDefault(re => re.Caption == cpu.CurrencyPair.Symbol);
			if (element != null)
				_ccyPairs.Elements.Remove(element);
		}

		static void RefreshElement (Element element)
		{
			if (element.GetContainerTableView () != null) {
				var root = element.GetImmediateRootElement ();
				if (root != null) {
					root.Reload (element, UITableViewRowAnimation.Fade);
				}
			}
		}
	}
}

