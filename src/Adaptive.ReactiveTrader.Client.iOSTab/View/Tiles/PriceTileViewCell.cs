
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Shared.DTO.ReferenceData;
using System.ComponentModel;
using MonoTouch.CoreGraphics;
using Adaptive.ReactiveTrader.Client.iOSTab.Tiles;
using System.IO;
using Adaptive.ReactiveTrader.Client.UI.SpotTiles;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public partial class PriceTileViewCell : UITableViewCell, IPriceTileCell, INotionalDelegateRecipient
	{
		public static readonly UINib Nib = UINib.FromName ("PriceTileViewCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("PriceTileViewCell");

		private PriceTileModel _priceTileModel;


		public PriceTileViewCell (IntPtr handle) : base (handle)
		{
		}

		public static PriceTileViewCell Create ()
		{
			PriceTileViewCell created = (PriceTileViewCell)Nib.Instantiate (null, null) [0];
			created.ContentView.BackgroundColor = Styles.RTDarkerBlue;

			created.Notional.Delegate = new NotionalTextFieldDelegate (created);

			var numberToolbar = new UIToolbar (new RectangleF(0.0f, 0.0f, created.Frame.Size.Width, 40.0f));

			numberToolbar.Items = new UIBarButtonItem[] {
				new UIBarButtonItem ("Cancel", UIBarButtonItemStyle.Bordered, created, new MonoTouch.ObjCRuntime.Selector ("CancelNumberPad")),
				new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace),
				new UIBarButtonItem (UIBarButtonSystemItem.Done, created, new MonoTouch.ObjCRuntime.Selector ("DoneNumberPad"))
			};

			created.Notional.InputAccessoryView = numberToolbar;

			return created;
		}

		[Export("CancelNumberPad")]
		private void CancelNumberPad ()
		{
			System.Console.WriteLine ("CancelNumberPad");
			this.Notional.Text = Styles.FormatNotional (_priceTileModel.Notional, false);
			this.Notional.ResignFirstResponder ();
		}

		[Export("DoneNumberPad")]
		private void DoneNumberPad ()
		{
			System.Console.WriteLine ("DoneNumberPad");
			this.Notional.ResignFirstResponder ();
		}

		public void UpdateFrom (PriceTileModel model)
		{
			_priceTileModel = model;

			this.CurrencyPair.Text = model.Symbol;

			SetBuySellSides (model);

			if (this.Notional.IsEditing) {
				// TODO: Determine what it really means to update the Notional via the model.
				System.Console.WriteLine ("Notional {0} is mid edit, so not updating to {1}", this.Notional.Text, model.Notional);
			} else {
				this.Notional.Text = Styles.FormatNotional (model.Notional, true);
			}

			this.LeftSideNumber.Text = model.LeftSideNumber;
			this.LeftSideBigNumber.Text = model.LeftSideBigNumber;
			this.LeftSidePips.Text = model.LeftSidePips;

			this.RightSideNumber.Text = model.RightSideNumber;
			this.RightSideBigNumber.Text = model.RightSideBigNumber;
			this.RightSidePips.Text = model.RightSidePips;
				
			this.Executing.Hidden = model.Status != PriceTileStatus.Executing;

			this.Spread.Text = model.Spread;

			switch (model.Movement) {
				case PriceMovement.Down:
					this.PriceMovementDown.Hidden = false;
					this.PriceMovementUp.Hidden = true;
					break;
				case PriceMovement.Up:
					this.PriceMovementDown.Hidden = true;
					this.PriceMovementUp.Hidden = false;
					break;
				case PriceMovement.None:
					this.PriceMovementDown.Hidden = true;
					this.PriceMovementUp.Hidden = true;
					break;
			}
			model.Rendered ();
		}

		partial void LeftSideButtonTouchUpInside (NSObject sender)
		{
			// TODO: Reconsider the UX here. Should we make use of a partly-entered notional value? One that might be rejected?

			if (this.Notional.IsEditing) {
				// Try to accept whatever value's been entered, and then use it below...
				DoneNumberPad();
			}

			// TODO 
			var model = _priceTileModel;
			if (model != null && model.Status == PriceTileStatus.Streaming) {
				//
				// TODO: Determine where to best place the check for trading enabled.
				// Where would we implenet two-touch, or more complex order entry?
				//
				if (UserModel.Instance.OneTouchTradingEnabled && model.Bid()) {
					UserModel.Instance.OneTouchTradingEnabled = false;
				}
			}
		}

		partial void RightSideButtonTouchUpInside (NSObject sender)
		{
			// TODO: Reconsider the UX here. Should we make use of a partly-entered notional value? One that might be rejected?

			if (this.Notional.IsEditing) {
				// Try to accept whatever value's been entered, and then use it below...
				DoneNumberPad();
			}

			var model = _priceTileModel;
			if (model != null && model.Status == PriceTileStatus.Streaming) {
				if (UserModel.Instance.OneTouchTradingEnabled && model.Ask()) {
					UserModel.Instance.OneTouchTradingEnabled = false;
				}
			}
		}

		public void NotionalValueEditStarted()
		{
			System.Console.WriteLine("NotionalValueEditStarted, now {0}", Notional.Text);
		}

		public void NotionalValueEditComplete ()
		{
			System.Console.WriteLine("NotionalValueEditComplete, now {0}", Notional.Text);

			var model = _priceTileModel;
			if (model != null) {
				long newNotional;
				if (long.TryParse (Notional.Text, out newNotional)) {
					model.Notional = newNotional;
				} else {
					System.Console.WriteLine("Unable to parse notional {0}", Notional.Text);
					// Leave model value unchanged.
					// TODO: A more elegant failure case for unparsable (or otherwise unsuitable) notional?
					// TODO: Consider canning any attempt at one-touch trading which triggered this parse attempt.
				}
			}

			// Reinstate nicely-formatted version of notional (with new, or existing, value)...

			this.Notional.Text = Styles.FormatNotional (_priceTileModel.Notional, true);
		}
			
		partial void NotionalCcyTouchUpInside (NSObject sender)
		{
			var model = _priceTileModel;
			if (model != null) {
				SwapBuySellSides (model);
				SetBuySellSides (model);
			}
		}

		void SwapBuySellSides(PriceTileModel model) {
			if (model.NotionalCcy == model.Base) {
				model.NotionalCcy = model.Counter;
			} else {
				model.NotionalCcy = model.Base;
			}
		}

		void SetBuySellSides (PriceTileModel model)
		{
			if (model.NotionalCcy == model.Base) {
				this.LeftSideAction.Text = "SELL";
				this.RightSideAction.Text = "BUY";
			}
			else {
				this.LeftSideAction.Text = "BUY";
				this.RightSideAction.Text = "SELL";
			}

			this.NotionalCCY.SetTitle (model.NotionalCcy, UIControlState.Normal);
			this.NotionalCCY.SetTitle (model.NotionalCcy, UIControlState.Disabled);
			this.NotionalCCY.SetTitle (model.NotionalCcy, UIControlState.Selected);

			// Show what it WILL be, when user is mid tap...

			this.NotionalCCY.SetTitle ((model.NotionalCcy == model.Base)?model.Counter:model.Base, UIControlState.Highlighted);
		}
	}
}

