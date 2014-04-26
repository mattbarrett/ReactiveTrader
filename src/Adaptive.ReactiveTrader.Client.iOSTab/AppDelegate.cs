using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;

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
			_reactiveTrader.Initialize ("trader", new [] { "http://reactivetrader.azurewebsites.net/signalr" });
			_reactiveTrader.ConnectionStatusStream
				.Subscribe (ci => {
					BeginInvokeOnMainThread(() => {
					var view = new UIAlertView() {
						Title = "Connection Status",
						Message = string.Format("Reactive Trader connection status is now {0}.", ci.ConnectionStatus.ToString())
					};
					view.AddButton("OK");
					view.Show();
					});
				});

			var viewController1 = new FirstViewController (_reactiveTrader);
			var viewController2 = new SecondViewController (_reactiveTrader);
			tabBarController = new UITabBarController ();
			tabBarController.ViewControllers = new UIViewController [] {
				viewController1,
				viewController2,
			};


			window.RootViewController = tabBarController;
			// make the window visible
			window.MakeKeyAndVisible ();
			
			return true;
		}
	}
}

