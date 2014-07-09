
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.iOSTab.Model;

namespace Adaptive.ReactiveTrader.Client.iOSTab.Tiles
{
	public partial class PriceTileTradeAffirmationViewCell : UITableViewCell, IPriceTileCell, ITradeDoneCell
	{
		public static readonly UINib Nib = UINib.FromName ("PriceTileTradeAffirmationViewCell", NSBundle.MainBundle);
		public static readonly NSString Key = new NSString ("PriceTileTradeAffirmationViewCell");

		PriceTileModel _priceTileModel;

		public PriceTileTradeAffirmationViewCell (IntPtr handle) : base (handle)
		{
		}

		public static PriceTileTradeAffirmationViewCell Create ()
		{
			PriceTileTradeAffirmationViewCell created = (PriceTileTradeAffirmationViewCell)Nib.Instantiate (null, null) [0];
			created.ContentView.BackgroundColor = Styles.RTDarkerBlue;
			return created;
		}

		partial void Done (NSObject sender)
		{
			var model = _priceTileModel;
			if (model != null) {
				model.Done();
			}
		}

		public void UpdateFrom (PriceTileModel model)
		{
			_priceTileModel = model;

			var tradeDone = model.TradeDone;
			if (tradeDone == null) {
				return;
			}

			UpdateFrom (tradeDone);

			DoneButton.Hidden = false;
		}

		public void UpdateFrom(TradeDoneModel model) {

			UIStringAttributes strikethroughAttributes;

			switch (model.Trade.TradeStatus) {
			case Domain.Models.Execution.TradeStatus.Rejected:
				strikethroughAttributes = new UIStringAttributes {
					StrikethroughStyle = NSUnderlineStyle.Single
				};
				break;

			case Domain.Models.Execution.TradeStatus.Done:
			default:
				strikethroughAttributes = new UIStringAttributes {
					StrikethroughStyle = NSUnderlineStyle.None
				};
				break;
			}

			// Always displayed 'plain'...

			CurrencyPair.Text = model.Trade.CurrencyPair;
			Direction.Text = model.Trade.Direction.ToString ();
			TradeId.Text = model.Trade.TradeId.ToString ();


			// May be struck through in the event of trade failure...

			CounterCCY.AttributedText = new NSAttributedString(model.Trade.DealtCurrency, strikethroughAttributes);
			DirectionAmount.AttributedText = new NSAttributedString(Styles.FormatNotional(model.Trade.Notional, true), strikethroughAttributes);
			Rate.AttributedText = new NSAttributedString(model.Trade.SpotRate.ToString (), strikethroughAttributes);
			SpotDate.AttributedText = new NSAttributedString(model.Trade.ValueDate.ToShortDateString (), strikethroughAttributes);

			// May also be BOLD if the trader id matches the current user...

			UIStringAttributes maybeStrikeMaybeBold = new UIStringAttributes();
			maybeStrikeMaybeBold.StrikethroughStyle = strikethroughAttributes.StrikethroughStyle;
			if (model.Trade.TraderName == UserModel.Instance.TraderId) {
				maybeStrikeMaybeBold.Font = UIFont.BoldSystemFontOfSize(TraderId.Font.PointSize);
			}

			TraderId.AttributedText = new NSAttributedString(model.Trade.TraderName, maybeStrikeMaybeBold);

			// May also be struck through, but currently content not dynamic?

			DirectionCCY.AttributedText = new NSAttributedString(DirectionCCY.Text, strikethroughAttributes);

			DoneButton.Hidden = true;

		}
	}
}

