using System;
using System.Linq;
using System.Reactive.Linq;
using MonoTouch.Foundation;
using Adaptive.ReactiveTrader.Client.Domain.Models.ReferenceData;
using Adaptive.ReactiveTrader.Client.Domain.Models.Pricing;
using Adaptive.ReactiveTrader.Client.Concurrency;
using Adaptive.ReactiveTrader.Shared.DTO.Pricing;
using Adaptive.ReactiveTrader.Shared.Extensions;
using Adaptive.ReactiveTrader.Client.Domain.Models.Execution;
using MonoTouch.SystemConfiguration;
using System.Dynamic;
using Adaptive.ReactiveTrader.Client.iOSTab.Tiles;
using System.IO;
using Adaptive.ReactiveTrader.Shared.DTO.Execution;
using Adaptive.ReactiveTrader.Client.iOSTab.Model;
using Adaptive.ReactiveTrader.Client.UI.SpotTiles;
using Adaptive.ReactiveTrader.Client.Domain.Instrumentation;
using System.Runtime.InteropServices;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	public class PriceTileModel : NotifyingModel<PriceTileModel> // NSObject
	{
		private readonly ICurrencyPair _currencyPair;
		private readonly IConcurrencyService _concurrencyService;

		IPriceLatencyRecorder priceLatencyRecorder;

		IPrice _lastPrice;

		public PriceTileModel (ICurrencyPair currencyPair, IPriceLatencyRecorder priceLatencyRecorder, IConcurrencyService concurrencyService)
		{
			this.priceLatencyRecorder = priceLatencyRecorder;
			this._currencyPair = currencyPair;
			this._concurrencyService = concurrencyService;

			// default ui content
			this.Symbol = _currencyPair.BaseCurrency + " / " + _currencyPair.CounterCurrency;
			this.Status = PriceTileStatus.Streaming;
			this.RightSideBigNumber = this.LeftSideBigNumber = "--";
			this.Notional = 1000000;
			this.NotionalCcy = this.Base;

			_currencyPair.PriceStream
				.ObserveOn(_concurrencyService.Dispatcher)
				.Subscribe (price => OnPrice (price));
		}

		public PriceTileStatus Status { get; set; }

		public string Symbol { get;	set; }
		public string Base { get { return _currencyPair.BaseCurrency; } }
		public string Counter { get { return _currencyPair.CounterCurrency;}}

		public string LeftSideNumber { get; set; }
		public string LeftSideBigNumber { get; set; }
		public string LeftSidePips { get; set; }

		public string RightSideNumber  { get; set; }
		public string RightSideBigNumber  { get; set; }
		public string RightSidePips  { get; set; }

		public string Spread { get; set; }

		public long Notional { get; set; }
		public string NotionalCcy { get; set; }
		public TradeDoneModel TradeDone { get; set; }
		public PriceMovement Movement { get; set; }

		public Boolean Bid ()
		{
			Boolean executed = false;
			var price = _lastPrice;

			if (price != null) {

				Status = PriceTileStatus.Executing;

				price.Bid.ExecuteRequest ( Notional, _currencyPair.BaseCurrency)
					.SubscribeOn(_concurrencyService.TaskPool)
					.ObserveOn(_concurrencyService.Dispatcher)
					.Subscribe(OnTradeResponseUpdate);

				executed = true;

				NotifyOnChanged (this);

			}

			// TODO: Failure cases (eg unsuitable notional).

			return (executed);
		}

		public Boolean Ask ()
		{
			Boolean executed = false;
			var price = _lastPrice;

			if (price != null) {

				Status = PriceTileStatus.Executing;

				price.Ask.ExecuteRequest ( Notional, _currencyPair.BaseCurrency)
					.SubscribeOn(_concurrencyService.TaskPool)
					.ObserveOn(_concurrencyService.Dispatcher)
					.Subscribe(OnTradeResponseUpdate);

				executed = true;

				NotifyOnChanged (this);
			}

			// TODO: Failure cases (eg unsuitable notional).

			return (executed);
		}

		public void Done() {
			Status = PriceTileStatus.Streaming;
			NotifyOnChanged (this);
		}

		public void Rendered() {
			this.priceLatencyRecorder.OnRendered (_lastPrice);
		}

		private void OnTradeResponseUpdate(IStale<ITrade> tradeUpdate) {
			if (tradeUpdate.IsStale) {

			} else {
				var trade = tradeUpdate.Update;
				Status = PriceTileStatus.Done;
				TradeDone = new TradeDoneModel (trade);
			}
			NotifyOnChanged (this);
		}

		void OnPrice (IPrice currentPrice)
		{		
			if (!currentPrice.IsStale) {
				// TODO: Discuss other statuses (Done, Executing)...
				if (this.Status == PriceTileStatus.Stale) {
					this.Status = PriceTileStatus.Streaming;
				}

				var bid = PriceFormatter.GetFormattedPrice (currentPrice.Bid.Rate, currentPrice.CurrencyPair.RatePrecision, currentPrice.CurrencyPair.PipsPosition);
				var ask = PriceFormatter.GetFormattedPrice (currentPrice.Ask.Rate, currentPrice.CurrencyPair.RatePrecision, currentPrice.CurrencyPair.PipsPosition);

				LeftSideNumber = bid.BigFigures;
				LeftSideBigNumber = bid.Pips;
				LeftSidePips = bid.TenthOfPip;

				RightSideNumber = ask.BigFigures;
				RightSideBigNumber = ask.Pips;
				RightSidePips = ask.TenthOfPip;

				Spread = currentPrice.Spread.ToString ("0.0");

				if (_lastPrice != null && !_lastPrice.IsStale) {
					if (_lastPrice.Mid < currentPrice.Mid) {
						Movement = PriceMovement.Up;
					} else if (_lastPrice.Mid > currentPrice.Mid) {
						Movement = PriceMovement.Down;
					} else 
					{
						Movement = PriceMovement.None;
					}
				} else {
					Movement = PriceMovement.None;
				}
				this.NotifyOnChanged (this);
			} else {
				Movement = PriceMovement.None;

				// TODO: Discuss other statuses (Done, Executing)...
				if (this.Status == PriceTileStatus.Streaming) {
					this.Status = PriceTileStatus.Stale;
					this.NotifyOnChanged (this);
				}
			}
			_lastPrice = currentPrice;
		}
	}
}

