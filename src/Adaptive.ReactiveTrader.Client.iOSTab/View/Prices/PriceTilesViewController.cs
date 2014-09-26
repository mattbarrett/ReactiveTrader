
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Adaptive.ReactiveTrader.Client.Domain;
using Adaptive.ReactiveTrader.Client.Concurrency;
using System.Linq;
using Adaptive.ReactiveTrader.Client.iOSTab.Tiles;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using MonoTouch.ExternalAccessory;

namespace Adaptive.ReactiveTrader.Client.iOSTab
{
	//[Register("PriceTilesViewController")]
	public partial class PriceTilesViewController : UITableViewController
	{
		private readonly IReactiveTrader _reactiveTrader;
		private readonly IConcurrencyService _concurrencyService;
		private readonly PriceTilesModel _model;
		private readonly Dictionary<PriceTileModel, IDisposable> _subscriptions = new Dictionary<PriceTileModel, IDisposable>();

		public PriceTilesViewController (IReactiveTrader reactiveTrader, IConcurrencyService concurrencyService) 
			: base(UITableViewStyle.Plain)
		{
			this._concurrencyService = concurrencyService;
			this._reactiveTrader = reactiveTrader;

			Title = "Prices";
			TabBarItem.Image = UIImage.FromBundle ("tab_prices");

			_model = new PriceTilesModel (_reactiveTrader, _concurrencyService);

			_model.ActiveCurrencyPairs.CollectionChanged += (sender, e) => {

				switch (e.Action) {
				case NotifyCollectionChangedAction.Add: 
					foreach (var model in e.NewItems.Cast<PriceTileModel>()) {
						_subscriptions.Add (model, model.OnChanged.Subscribe (OnItemChanged));
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					foreach (var model in e.OldItems.Cast<PriceTileModel> ()) {
						IDisposable subscription;
						if (_subscriptions.TryGetValue (model, out subscription)) {
							subscription.Dispose();
							_subscriptions.Remove (model);
						}
					}
					break;
				}

				if (IsViewLoaded) {
					if (e.NewItems.Count == 1){
						TableView.InsertRows (
							new [] {
								NSIndexPath.Create (0, e.NewStartingIndex)
							}, UITableViewRowAnimation.Top);
//					} else if (e.OldItems.Count == 1) {
//						TableView.DeleteRows (new [] {
//							NSIndexPath.Create (0, e.OldStartingIndex)
//						}, UITableViewRowAnimation.Fade);
					} else {
						TableView.ReloadData();
					}
				}
			};

			_model.Initialise ();

		}

		private void OnItemChanged(PriceTileModel itemModel) {

			if (IsViewLoaded) {
				var indexOfItem = _model.ActiveCurrencyPairs.IndexOf (itemModel);

				NSIndexPath path = NSIndexPath.FromRowSection(indexOfItem, 0);
				IPriceTileCell cell = (IPriceTileCell)TableView.CellAt (path);

				if (cell == null) {
					//					System.Console.WriteLine ("Row {0} not found", indexOfItem);
					// There's no cell bound to that index in the data, so we can ignore the update.
				} else {
					//					System.Console.WriteLine ("Row {0} FOUND {1}", indexOfItem, cell.GetType ().ToString ());

					bool bAppropriateCell = false; // TODO: Refactor this elsewhere.

					switch (itemModel.Status) {
					case PriceTileStatus.Done:
					case PriceTileStatus.DoneStale:
						if (cell.GetType () == typeof(PriceTileTradeAffirmationViewCell))
						{
							bAppropriateCell = true;
						}
						break;

					case PriceTileStatus.Streaming:
					case PriceTileStatus.Executing:
						if (cell.GetType () == typeof(PriceTileViewCell)) {
							bAppropriateCell = true;
						}
						break;

					case PriceTileStatus.Stale:
						if (cell.GetType () == typeof(PriceTileErrorViewCell)) {
							bAppropriateCell = true;
						}
						break;
					}

					// TODO: Batch the updates up, to only call ReloadRows once per main event loop loop?

					if (bAppropriateCell) {
						//						System.Console.WriteLine ("Cell is APPROPRIATE", indexOfItem);
						cell.UpdateFrom (itemModel);
					} else {
						// TODO: If the cell is of the wrong type, reload the row instead.

						TableView.ReloadRows (
							new [] {
								NSIndexPath.Create (0, indexOfItem)
							}, UITableViewRowAnimation.Fade);
					}
				}

			}
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

			TableView.RegisterNibForHeaderFooterViewReuse (PricesHeaderCell.Nib, PricesHeaderCell.Key);

			TableView.Source = new PriceTilesViewSource (_model);

			Styles.ConfigureTable (TableView);
		}


		// Workaround: Prevent UI from incorrectly extending under tab bar.

		public override UIRectEdge EdgesForExtendedLayout {
			get {
				return (base.EdgesForExtendedLayout ^ UIRectEdge.Bottom);
			}
		}

	}
}

