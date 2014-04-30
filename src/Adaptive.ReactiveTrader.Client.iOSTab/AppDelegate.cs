using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;
using System.Diagnostics;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		private IReactiveTrader _reactiveTrader;

		// class-level declarations
		UIWindow window;
		UITabBarController tabBarController;
		//
		// This method is invoked when the application has loaded and is ready to run. In this
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// create a new window instance based on the screen size
			window = new UIWindow (UIScreen.MainScreen.Bounds);

			_reactiveTrader = new Adaptive.ReactiveTrader.Client.Domain.ReactiveTrader ();
			_reactiveTrader.Initialize ("iOS-" + Process.GetCurrentProcess ().Id, new [] { "http://reactivetrader.azurewebsites.net/signalr" });
			_reactiveTrader.ConnectionStatusStream
				.Subscribe (ci => BeginInvokeOnMainThread (() => {
				var view = new UIAlertView () {
					Title = "Connection Status",
					Message = string.Format ("Reactive Trader is now {0}.", ci.ConnectionStatus.ToString ().ToLowerInvariant ())
				};
				view.AddButton ("OK");
				view.Show ();
			}));

			var cs = new ConcurrencyService ();

			var viewController1 = new TradesViewController (_reactiveTrader);
			var viewController2 = new PricesViewController (_reactiveTrader, cs);
			#if DEBUG
			var logViewController = new LogViewController(_reactiveTrader, cs);
			#endif
			tabBarController = new UITabBarController ();
			tabBarController.ViewControllers = new UIViewController [] {
				viewController1,
				viewController2,
				#if DEBUG
				logViewController
				#endif
			};


			window.RootViewController = tabBarController;
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

