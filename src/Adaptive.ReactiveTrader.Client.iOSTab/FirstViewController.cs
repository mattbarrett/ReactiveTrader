using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Domain.Models.Execution;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public partial class FirstViewController : UIViewController
	{
		private IReactiveTrader _reactiveTrader;

		public FirstViewController (IReactiveTrader reactiveTrader) : base ("FirstViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Prices", "Prices");
			TabBarItem.Image = UIImage.FromBundle ("first");

			_reactiveTrader = reactiveTrader;

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


			var root = new RootElement ("Trades");
			var trades = new Section ("Trades");
			root.Add (trades);

			_reactiveTrader.TradeRepository.GetTradesStream()
				.Subscribe(delegate (IEnumerable<ITrade> tradesUpdate) {
					foreach (var trade in tradesUpdate) {
						trades.Add(new MultilineElement(trade.TradeStatus.ToString(), trade.ToString()));
					}
				});

			this.AddChildViewController (new DialogViewController (root));

			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}

