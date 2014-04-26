using System;
using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public partial class SecondViewController : UIViewController
	{
		IReactiveTrader _reactiveTrader;

		public SecondViewController (IReactiveTrader reactiveTrader) : base ("SecondViewController", null)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Trades", "Trades");
			TabBarItem.Image = UIImage.FromBundle ("second");

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
		}

		private IObservable<string> Create() {
			return Observable.Create<string> (o => {
				o.OnNext("hello");
				o.OnCompleted();
				return Disposable.Empty;
			});
		}
	}
}

