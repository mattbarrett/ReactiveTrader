using System;
using Adaptive.ReactiveTrader.Client.Domain;
using MonoTouch.UIKit;
using System.Drawing;

namespace Adaptive.ReactiveTrader.Client.iOS
{
    public class MyViewController : UIViewController
    {
        private readonly IObservable<ConnectionInfo> _connectionStatusStream;
        UIButton button;
        int numClicks = 0;
        float buttonWidth = 200;
        float buttonHeight = 50;

        public MyViewController(IObservable<ConnectionInfo> connectionStatusStream)
        {
            _connectionStatusStream = connectionStatusStream;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            button = UIButton.FromType(UIButtonType.RoundedRect);

            button.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                View.Frame.Height / 2 - buttonHeight / 2,
                buttonWidth,
                buttonHeight);

            button.SetTitle("Click me", UIControlState.Normal);

            _connectionStatusStream
                .Subscribe(ci => BeginInvokeOnMainThread(() => button.SetTitle(ci.ConnectionStatus.ToString(), UIControlState.Normal)));
            
            button.TouchUpInside += (object sender, EventArgs e) =>
            {
                button.SetTitle(String.Format("clicked {0} times", numClicks++), UIControlState.Normal);
            };

            button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button);
        }

    }
}

