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
	public partial class TradesViewController : UIViewController
	{
		private IReactiveTrader _reactiveTrader;

		public TradesViewController (IReactiveTrader reactiveTrader) : base ("FirstViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Trades", "Trades");
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

			_reactiveTrader.TradeRepository.GetTradesStream ()
				.Subscribe ((IEnumerable<ITrade> tradesUpdate) => BeginInvokeOnMainThread (() => {
				foreach (var trade in tradesUpdate) {
						trades.Insert (0, 
							UITableViewRowAnimation.Top
							new MultilineElement (trade.TradeStatus.ToString (), trade.ToString ()));
				}
			}));

			var dvc = new DialogViewController (root);
			this.AddChildViewController (dvc);
			this.Add (dvc.TableView);


			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}

